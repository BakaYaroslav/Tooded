using Microsoft.EntityFrameworkCore;

namespace Tooded_2.Shop;

internal sealed class ShopAdminForm : LocalizedForm
{
    private DataGridView productGrid = null!, offerGrid = null!;
    private List<Toode> products = new();
    private ShopConfiguration configuration = new();

    public ShopAdminForm()
    {
        LanguageManager.Bind(this, "Text", () => "shoping67 · " + UiText.Get("Management"));
        Size = new Size(1100, 750); MinimumSize = new Size(950, 600);
        Font = new Font("Segoe UI", 10); StartPosition = FormStartPosition.CenterParent;
        Build(); Reload();
    }
    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, AutoGenerateColumns = true };
    private void Build()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var productPage = new TabPage(UiText.Get("ProductsAndSpecifications"));
        var offerPage = new TabPage(UiText.Get("OffersAndPromotions"));
        productGrid = Grid(); offerGrid = Grid();
        productGrid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex >= 0 && productGrid.Columns[e.ColumnIndex].Name == "Category" && e.Value is string key)
            { e.Value = UiText.Category(key); e.FormattingApplied = true; }
        };
        productPage.Name = "productPage"; offerPage.Name = "offerPage";
        productPage.Controls.Add(productGrid); offerPage.Controls.Add(offerGrid);
        var productActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
        var edit = Shop67Ui.Button(()=>UiText.Get("EditSelectedProduct"), EditProduct, true); edit.Width = 270;
        productActions.Controls.Add(edit);
        var delete = Shop67Ui.Button(()=>UiText.Get("DeleteProduct"), DeleteProduct); delete.AccessibleName = "delete-product";
        delete.ForeColor = Color.Firebrick; productActions.Controls.Add(delete);
        productActions.Controls.Add(Shop67Ui.Button(()=>UiText.Get("Refresh"), Reload));
        productActions.Controls.Add(Shop67Ui.Button(()=>UiText.Get("EmailSettings"), () => { using var settings = new MailSettingsForm(); settings.ShowDialog(this); }));
        productPage.Controls.Add(productActions);
        var offerActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
        offerActions.Controls.Add(Shop67Ui.Button(()=>UiText.Get("AddPromotion"), () => EditOffer(null), true));
        offerActions.Controls.Add(Shop67Ui.Button(()=>UiText.Get("Refresh"), () => EditOffer(SelectedOffer())));
        offerActions.Controls.Add(Shop67Ui.Button(()=>UiText.Get("Delete"), DeleteOffer));
        offerPage.Controls.Add(offerActions);
        productGrid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditProduct(); };
        offerGrid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditOffer(SelectedOffer()); };
        tabs.TabPages.Add(productPage); tabs.TabPages.Add(offerPage); Controls.Add(tabs);
    }
    private void Reload()
    {
        using var db = new AppDbContext(); configuration = Shop67Settings.Load(db);
        products = db.Toodetabel.AsNoTracking().OrderBy(p => p.Id).ToList();
        productGrid.DataSource = products.Select(p => new { Id = p.Id, Name = p.Toodenimetus,
            Category = Shop67Settings.Details(configuration, p).Category, Price = p.Hind, Stock = p.Kogus,
            Visible = Shop67Settings.Details(configuration, p).Visible }).ToList();
        offerGrid.DataSource = configuration.Promotions.OrderBy(p => p.Position).Select(p => new { Id = p.Id,
            Title = p.Title.Value, Discount = p.Percent, Starts = p.Starts.LocalDateTime, Ends = p.Ends.LocalDateTime,
            Active = p.Active(DateTimeOffset.UtcNow), Position = p.Position }).ToList();
        if (offerGrid.Columns.Contains("Id")) offerGrid.Columns["Id"].Visible = false;
        Headers(productGrid); Headers(offerGrid);
    }
    private static void Headers(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns) column.HeaderText = column.Name switch
        {
            "Name" => UiText.Get("Name"), "Category" => UiText.Get("Category"),
            "Price" => UiText.Get("Price"), "Stock" => UiText.Get("Stock"),
            "Visible" => UiText.Get("Visible"), "Title" => UiText.Get("Title"),
            "Discount" => UiText.Get("Discount"), "Starts" => UiText.Get("Starts"),
            "Ends" => UiText.Get("Ends_C1B494"), "Active" => UiText.Get("Active"),
            "Position" => UiText.Get("DisplayOrder"), _ => column.Name
        };
    }
    protected override void RefreshLocalizedValues()
    {
        Headers(productGrid); Headers(offerGrid);
        productGrid.Invalidate(); offerGrid.Invalidate();
    }
    private Promotion? SelectedOffer() => offerGrid.CurrentRow == null ? null : configuration.Promotions.FirstOrDefault(p => p.Id == Convert.ToString(offerGrid.CurrentRow.Cells["Id"].Value));
    private void EditProduct()
    {
        if (productGrid.CurrentRow == null) return;
        int id = Convert.ToInt32(productGrid.CurrentRow.Cells["Id"].Value);
        using var editor = new ProductEditorForm(id); if (editor.ShowDialog(this) == DialogResult.OK) Reload();
    }
    private void DeleteProduct()
    {
        if (productGrid.CurrentRow == null)
        { MessageBox.Show(this, UiText.Get("SelectAProductInTheTable_C673E3"), "shoping67"); return; }
        int id = Convert.ToInt32(productGrid.CurrentRow.Cells["Id"].Value);
        string name = Convert.ToString(productGrid.CurrentRow.Cells["Name"].Value) ?? "";
        if (MessageBox.Show(this, UiText.Format("DeleteProduct0FromTheDatabaseAutomaticImportWillNotRestoreItExis", name),
            UiText.Get("DeleteProduct"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        try
        {
            if (!Shop67Settings.DeleteProduct(id)) MessageBox.Show(this, UiText.Get("TheProductHasAlreadyBeenDeleted"), "shoping67");
            Reload();
        }
        catch (Exception)
        { MessageBox.Show(this, UiText.Get("UnableToDeleteTheProductCheckTheDatabaseConnectionAndTryAgain"), "shoping67", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void EditOffer(Promotion? offer)
    {
        using var editor = new PromotionEditorForm(offer ?? new Promotion(), products, configuration);
        if (editor.ShowDialog(this) != DialogResult.OK) return;
        using var db = new AppDbContext(); var current = Shop67Settings.Load(db);
        var catalog = db.Toodetabel.AsNoTracking().ToList().Select(p => new ShopProduct
            { Id = p.Id, Name = p.Toodenimetus, Details = Shop67Settings.Details(current, p) }).ToArray();
        try { PromotionTargets.Validate(editor.Value, catalog); }
        catch (InvalidOperationException ex) { MessageBox.Show(this, ex.Message, "shoping67"); return; }
        current.Promotions.RemoveAll(p => p.Id == editor.Value.Id); current.Promotions.Add(editor.Value);
        current.Initialized = true; Shop67Settings.Save(db, current); Reload();
    }
    private void DeleteOffer()
    {
        var offer = SelectedOffer(); if (offer == null) return;
        if (MessageBox.Show(this, UiText.Get("DeleteThisPromotion"), "shoping67", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var db = new AppDbContext(); var current = Shop67Settings.Load(db);
        current.Promotions.RemoveAll(p => p.Id == offer.Id); Shop67Settings.Save(db, current); Reload();
    }
}

internal abstract class ShopEditorForm : LocalizedForm
{
    protected readonly FlowLayoutPanel Fields = ShopStyle.Stack();
    protected ShopEditorForm(Func<string> title)
    {
        LanguageManager.Bind(this, "Text", () => "shoping67 · " + title()); Size = new Size(820, 820); MinimumSize = new Size(700, 600);
        Font = new Font("Segoe UI", 10); StartPosition = FormStartPosition.CenterParent;
        Fields.Padding = new Padding(20); Controls.Add(Fields);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(20, 8, 0, 0) };
        actions.Controls.Add(Shop67Ui.Button(()=>UiText.Get("Save"), () =>
        {
            try { Save(); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "shoping67"); }
        }, true));
        actions.Controls.Add(Shop67Ui.Button(()=>UiText.Get("Cancel"), Close)); Controls.Add(actions);
    }
    protected abstract void Save();
    protected TextBox TextField(Func<string> label, string value, bool multiline = false)
    {
        Fields.Controls.Add(Shop67Ui.Label(label, 10, true));
        var field = new TextBox { Text = value, Width = 710, Multiline = multiline, Height = multiline ? 75 : 28, ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None };
        Fields.Controls.Add(field); return field;
    }
    protected TextBox[] ContentField(Func<string> label, CatalogText value, bool multiline = false)
    {
        var field = TextField(label, value.Et, multiline);
        field.Tag = value;
        return new[] { field };
    }
    protected static CatalogText Read(TextBox[] fields) =>
        (fields[0].Tag as CatalogText ?? new()) with { Et = fields[0].Text.Trim() };
    protected NumericUpDown Number(Func<string> label, decimal value, decimal maximum, int decimals = 0)
    {
        Fields.Controls.Add(Shop67Ui.Label(label, 10, true));
        var field = new NumericUpDown { Maximum = maximum, DecimalPlaces = decimals, Value = Math.Clamp(value, 0, maximum), Width = 180 };
        Fields.Controls.Add(field); return field;
    }
    protected ComboBox Choice(Func<string> label, IEnumerable<string> keys, string value, Func<string, string> display)
    {
        Fields.Controls.Add(Shop67Ui.Label(label, 10, true));
        var field = new ComboBox { Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var key in keys) field.Items.Add(new ChoiceItem(key, "") { Display = () => display(key) });
        field.SelectedIndex = Enumerable.Range(0, field.Items.Count).FirstOrDefault(i => ((ChoiceItem)field.Items[i]).Key == value);
        LanguageManager.OnRefresh(field, () =>
        {
            int selected = field.SelectedIndex;
            for (int i = 0; i < field.Items.Count; i++) field.Items[i] = field.Items[i];
            field.SelectedIndex = selected;
        });
        Fields.Controls.Add(field); return field;
    }
    protected static string Key(ComboBox field) => (field.SelectedItem as ChoiceItem)?.Key ?? "";
    protected sealed record ChoiceItem(string Key, string Label)
    { public Func<string>? Display { get; init; } public string DisplayLabel => Display?.Invoke() ?? Label; public override string ToString() => DisplayLabel; }
}

internal sealed class PromotionEditorForm : ShopEditorForm
{
    private readonly Promotion original;
    private readonly TextBox[] titles, descriptions;
    private readonly NumericUpDown percent, position;
    private readonly DateTimePicker starts, ends;
    private readonly CheckBox enabled;
    private readonly ComboBox category;
    private readonly TextBox image;
    private readonly CheckedListBox selectedProducts;
    private readonly CheckBox allProducts;
    private readonly Label targetSummary;
    private readonly IReadOnlyList<ShopProduct> targetCatalog;
    public Promotion Value { get; private set; }
    public PromotionEditorForm(Promotion value, IReadOnlyList<Toode> products, ShopConfiguration config)
        : base(() => UiText.Get("Promotion"))
    {
        original = Value = value;
        targetCatalog = products.Select(p => new ShopProduct { Id = p.Id, Name = p.Toodenimetus,
            Details = Shop67Settings.Details(config, p) }).ToArray();
        titles = ContentField(()=>UiText.Get("Title"), value.Title);
        descriptions = ContentField(()=>UiText.Get("Description"), value.Description, true);
        category = Choice(()=>UiText.Get("Category"), CatalogRules.Categories.Prepend(""), value.Category, UiText.Category);
        category.AccessibleName = "promotion-category";
        percent = Number(()=>UiText.Get("Discount0CollectionWithoutDiscount"), value.Percent, 90, 2);
        position = Number(()=>UiText.Get("DisplayOrder"), value.Position, 999);
        Fields.Controls.Add(Shop67Ui.Label(()=>UiText.Get("StartEndLocalTime"), 10, true));
        starts = new DateTimePicker { Value = value.Starts.LocalDateTime, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm", Width = 250 };
        ends = new DateTimePicker { Value = value.Ends.LocalDateTime, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm", Width = 250 };
        Fields.Controls.Add(starts); Fields.Controls.Add(ends);
        enabled = LanguageManager.Bind(new CheckBox { Checked = value.Enabled,Width = 300 }, "Text", () => UiText.Get("Published")); Fields.Controls.Add(enabled);
        image = TextField(()=>UiText.Get("BannerImageHTTPSURLOptional"), value.Image);
        allProducts = LanguageManager.Bind(new CheckBox {             Checked = string.IsNullOrWhiteSpace(value.ProductKeys),Width = 710,Height = 32,AccessibleName = "promotion-all-products" }, "Text", () => UiText.Get("EntireSelectedCategory"));
        Fields.Controls.Add(allProducts);
        Fields.Controls.Add(Shop67Ui.Label(()=>UiText.Get("OrSelectIndividualProducts"), 10, true));
        selectedProducts = new CheckedListBox { Width = 710, Height = 200, CheckOnClick = true, AccessibleName = "promotion-products" };
        targetSummary = Shop67Ui.Label(()=>"", 10); targetSummary.MaximumSize = new Size(710, 0);
        targetSummary.AccessibleName = "promotion-target-summary";
        Fields.Controls.Add(selectedProducts); Fields.Controls.Add(targetSummary);
        var selected = value.ProductKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        void Populate(IEnumerable<string> requested)
        {
            var keys = requested.ToHashSet(StringComparer.OrdinalIgnoreCase);
            selectedProducts.Items.Clear();
            foreach (var product in PromotionTargets.Eligible(targetCatalog, Key(category)))
                selectedProducts.Items.Add(new ChoiceItem(product.Details.SourceKey, product.DisplayName), keys.Contains(product.Details.SourceKey));
            selectedProducts.Enabled = !allProducts.Checked;
            UpdateSummary();
        }
        category.SelectedIndexChanged += (_, _) => { if (!LanguageManager.IsApplying) Populate(selectedProducts.CheckedItems.Cast<ChoiceItem>().Select(p => p.Key).ToArray()); };
        allProducts.CheckedChanged += (_, _) => { selectedProducts.Enabled = !allProducts.Checked; UpdateSummary(); };
        selectedProducts.ItemCheck += (_, _) => { if (IsHandleCreated) BeginInvoke(new Action(UpdateSummary)); };
        Populate(selected);
    }
    private void UpdateSummary()
    {
        int count = allProducts.Checked ? selectedProducts.Items.Count : selectedProducts.CheckedItems.Count;
        targetSummary.ForeColor = count == 0 ? Color.Firebrick : ShopStyle.Muted;
        LanguageManager.Bind(targetSummary, "Text", () => count == 0 ? UiText.Get("SelectProductsOrTheEntireCategoryPreviousSelectionsMayBeHiddenOr")
            : string.Format(UiText.Get("PromotionAppliesTo0Products"), count));
    }

    protected override void Save()
    {
        if (image.Text.Length > 0 && (!Uri.TryCreate(image.Text, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            throw new InvalidOperationException(UiText.Get("UseAnHTTPSURL"));
        if (!allProducts.Checked && selectedProducts.CheckedItems.Count == 0)
            throw new InvalidOperationException(UiText.Get("SelectAtLeastOneProductOrEnableTheEntireCategory"));
        Value = original with { Title = Read(titles), Description = Read(descriptions), Category = Key(category),
            Percent = percent.Value, Position = (int)position.Value, Starts = new DateTimeOffset(starts.Value), Ends = new DateTimeOffset(ends.Value),
            Enabled = enabled.Checked, Image = image.Text.Trim(), ProductKeys = allProducts.Checked ? "" : string.Join(",", selectedProducts.CheckedItems.Cast<ChoiceItem>().Select(i => i.Key)) };
        PromotionTargets.Validate(Value, targetCatalog);
    }
}

internal sealed class ProductEditorForm : ShopEditorForm
{
    private readonly int productId;
    private readonly ProductDetails original;
    private readonly TextBox[] names, descriptions;
    private readonly TextBox brand, image;
    private readonly ComboBox category, kind;
    private readonly NumericUpDown price, stock;
    private readonly CheckBox visible;
    private readonly DataGridView specs;
    public ProductEditorForm(int id) : base(() => UiText.Get("ProductDetails"))
    {
        productId = id;
        using var db = new AppDbContext(); var product = db.Toodetabel.Find(id) ?? throw new InvalidOperationException(UiText.Get("ProductNotFound"));
        original = Shop67Settings.Details(Shop67Settings.Load(db), product);
        names = ContentField(()=>UiText.Get("Name"), original.Name.Et.Length == 0 ? new(product.Toodenimetus, product.Toodenimetus) : original.Name);
        descriptions = ContentField(()=>UiText.Get("Description"), original.Description, true);
        category = Choice(()=>UiText.Get("Category"), CatalogRules.Categories, original.Category, UiText.Category);
        kind = Choice(()=>UiText.Spec("kind"), new[] { "phone", "laptop", "desktop", "mouse", "keyboard", "headphones" }, original.Kind, UiText.Value);
        brand = TextField(()=>UiText.Spec("brand"), original.Brand);
        price = Number(()=>UiText.Get("RegularPrice"), (decimal)product.Hind, 1000000, 2);
        stock = Number(()=>UiText.Get("Stock"), product.Kogus, 1000000);
        visible = LanguageManager.Bind(new CheckBox { Checked = original.Visible,Width = 400 }, "Text", () => UiText.Get("ShowInShop")); Fields.Controls.Add(visible);
        image = TextField(()=>UiText.Get("ImageHTTPSURLOrLocalFile"), product.Pilt ?? "");
        Fields.Controls.Add(Shop67Ui.Label(()=>UiText.Get("SpecificationsEmptyValueUnknown"), 11, true));
        specs = new DataGridView { Width = 710, Height = 280, AllowUserToAddRows = true, AllowUserToDeleteRows = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
        var keys = new DataGridViewComboBoxColumn { Name = "Key", HeaderText = UiText.Get("Specification"), FlatStyle = FlatStyle.Flat };
        keys.DataSource = CatalogRules.Categories.SelectMany(CatalogRules.Facets).Concat(original.Specs.Keys).Distinct()
            .Where(k => k != "brand" && k != "kind").Select(k => new ChoiceItem(k, "") { Display = () => UiText.Spec(k) }).ToList();
        keys.ValueMember = "Key"; keys.DisplayMember = "DisplayLabel";
        specs.Columns.Add(keys); specs.Columns.Add("Value", UiText.Get("Value"));
        LanguageManager.OnRefresh(specs, () =>
        {
            specs.Columns["Key"].HeaderText = UiText.Get("Specification");
            specs.Columns["Value"].HeaderText = UiText.Get("Value");
            specs.Invalidate();
        });
        foreach (var pair in original.Specs) specs.Rows.Add(pair.Key, pair.Value);
        specs.CellFormatting += (_, e) => { if (e.ColumnIndex == 0 && e.Value is string key) { e.Value = UiText.Spec(key); e.FormattingApplied = true; } };
        specs.DataError += (_, e) => { e.ThrowException = false; };
        Fields.Controls.Add(specs);
    }
    protected override void Save()
    {
        specs.EndEdit();
        var translated = Read(names);
        if (string.IsNullOrWhiteSpace(translated.Et))
            throw new InvalidOperationException(UiText.Get("EnterAProductName"));
        var values = new Dictionary<string, string>();
        foreach (DataGridViewRow row in specs.Rows)
        {
            if (row.IsNewRow) continue;
            string key = Convert.ToString(row.Cells[0].Value)?.Trim() ?? "";
            string value = Convert.ToString(row.Cells[1].Value)?.Trim() ?? "";
            if (value.Length == 0) continue;
            if (key.Length == 0 || !values.TryAdd(key, value)) throw new InvalidOperationException(UiText.Get("ASpecificationIsMissingOrDuplicated"));
        }
        string selectedCategory = Key(category), selectedKind = Key(kind);
        if ((selectedCategory == "phones" && selectedKind != "phone") || (selectedCategory == "computers" && selectedKind is not ("laptop" or "desktop")) || (selectedCategory == "accessories" && selectedKind is not ("mouse" or "keyboard" or "headphones")))
            throw new InvalidOperationException(UiText.Get("TheDeviceTypeDoesNotMatchTheCategory"));
        using var db = new AppDbContext(); using var transaction = db.Database.BeginTransaction();
        var product = db.Toodetabel.Find(productId) ?? throw new InvalidOperationException(UiText.Get("ProductNotFound"));
        var config = Shop67Settings.Load(db);
        var detail = original with { Name = translated, Description = Read(descriptions), Brand = brand.Text.Trim(), Category = selectedCategory,
            Kind = selectedKind, Specs = values, Visible = visible.Checked, SourceKey = original.SourceKey.Length == 0 ? "local:" + productId : original.SourceKey };
        if (!CatalogRules.IsTechnical(new ShopProduct { Name = translated.Et, Details = detail }))
            throw new InvalidOperationException(UiText.Get("EnterARealProductModelManufacturerAndMatchingCategory"));
        if (detail.Visible && string.IsNullOrWhiteSpace(image.Text))
            throw new InvalidOperationException(UiText.Get("AVisibleProductMustHaveAnImage"));
        var dbCategory = db.Kategooriatabel.FirstOrDefault(c => c.nimetus == selectedCategory);
        if (dbCategory == null) { dbCategory = new Kategooria { nimetus = selectedCategory }; db.Kategooriatabel.Add(dbCategory); }
        product.Kategooria = dbCategory; product.Toodenimetus = translated.Et; product.Hind = (float)price.Value;
        product.Kogus = (int)stock.Value; product.Pilt = image.Text.Trim(); config.Products[productId] = detail;
        Shop67Settings.Save(db, config); transaction.Commit();
    }
}
