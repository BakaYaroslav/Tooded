using Microsoft.EntityFrameworkCore;

namespace Tooded_2.Shop;

internal sealed class ShopAdminForm : Form
{
    private DataGridView productGrid = null!, offerGrid = null!;
    private List<Toode> products = new();
    private ShopConfiguration configuration = new();

    public ShopAdminForm()
    {
        Text = "shoping67 · " + T.L("Haldus", "Управление", "Management");
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
        var productPage = new TabPage(T.L("Tooted ja omadused", "Товары и характеристики", "Products and specifications"));
        var offerPage = new TabPage(T.L("Pakkumised ja kampaaniad", "Предложения и акции", "Offers and promotions"));
        productGrid = Grid(); offerGrid = Grid();
        productPage.Controls.Add(productGrid); offerPage.Controls.Add(offerGrid);
        var productActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
        var edit = Shop67Ui.Button(T.L("Muuda valitud toodet", "Изменить выбранный товар", "Edit selected product"), EditProduct, true); edit.Width = 270;
        productActions.Controls.Add(edit);
        var delete = Shop67Ui.Button("Kustuta toode", DeleteProduct); delete.AccessibleName = "delete-product";
        delete.ForeColor = Color.Firebrick; productActions.Controls.Add(delete);
        productActions.Controls.Add(Shop67Ui.Button(T.Legacy("Uuenda"), Reload));
        productActions.Controls.Add(Shop67Ui.Button("E-posti seaded", () => { using var settings = new MailSettingsForm(); settings.ShowDialog(this); }));
        productPage.Controls.Add(productActions);
        var offerActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
        offerActions.Controls.Add(Shop67Ui.Button(T.L("Lisa kampaania", "Добавить акцию", "Add promotion"), () => EditOffer(null), true));
        offerActions.Controls.Add(Shop67Ui.Button(T.Legacy("Uuenda"), () => EditOffer(SelectedOffer())));
        offerActions.Controls.Add(Shop67Ui.Button(T.Legacy("Kustuta"), DeleteOffer));
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
            Category = T.Category(Shop67Settings.Details(configuration, p).Category), Price = p.Hind, Stock = p.Kogus,
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
            "Name" => T.L("Nimetus", "Название", "Name"), "Category" => T.L("Kategooria", "Категория", "Category"),
            "Price" => T.L("Hind", "Цена", "Price"), "Stock" => T.L("Laoseis", "Остаток", "Stock"),
            "Visible" => T.L("Nähtav", "Показывать", "Visible"), "Title" => T.L("Pealkiri", "Заголовок", "Title"),
            "Discount" => T.L("Allahindlus %", "Скидка %", "Discount %"), "Starts" => T.L("Algus", "Начало", "Starts"),
            "Ends" => T.L("Lõpp", "Окончание", "Ends"), "Active" => T.L("Aktiivne", "Действует", "Active"),
            "Position" => T.L("Järjekord", "Порядок", "Position"), _ => column.Name
        };
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
        { MessageBox.Show(this, "Vali tabelist toode.", "shoping67"); return; }
        int id = Convert.ToInt32(productGrid.CurrentRow.Cells["Id"].Value);
        string name = Convert.ToString(productGrid.CurrentRow.Cells["Name"].Value) ?? "";
        if (MessageBox.Show(this, $"Kas kustutada toode „{name}” andmebaasist?\n\nToode ei tule automaatse impordiga tagasi. Senised müügiandmed säilivad.",
            "Kustuta toode", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        try
        {
            if (!Shop67Settings.DeleteProduct(id)) MessageBox.Show(this, "Toode on juba kustutatud.", "shoping67");
            Reload();
        }
        catch (Exception)
        { MessageBox.Show(this, "Toodet ei saanud kustutada. Kontrolli andmebaasiühendust ja proovi uuesti.", "shoping67", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
        if (MessageBox.Show(this, T.L("Kustuta kampaania?", "Удалить акцию?", "Delete this promotion?"), "shoping67", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var db = new AppDbContext(); var current = Shop67Settings.Load(db);
        current.Promotions.RemoveAll(p => p.Id == offer.Id); Shop67Settings.Save(db, current); Reload();
    }
}

internal abstract class ShopEditorForm : Form
{
    protected readonly FlowLayoutPanel Fields = ShopStyle.Stack();
    protected ShopEditorForm(string title)
    {
        Text = "shoping67 · " + title; Size = new Size(820, 820); MinimumSize = new Size(700, 600);
        Font = new Font("Segoe UI", 10); StartPosition = FormStartPosition.CenterParent;
        Fields.Padding = new Padding(20); Controls.Add(Fields);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(20, 8, 0, 0) };
        actions.Controls.Add(Shop67Ui.Button(T.Legacy("Salvesta"), () =>
        {
            try { Save(); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "shoping67"); }
        }, true));
        actions.Controls.Add(Shop67Ui.Button(T.Legacy("Tühista"), Close)); Controls.Add(actions);
    }
    protected abstract void Save();
    protected TextBox TextField(string label, string value, bool multiline = false)
    {
        Fields.Controls.Add(Shop67Ui.Label(label, 10, true));
        var field = new TextBox { Text = value, Width = 710, Multiline = multiline, Height = multiline ? 75 : 28, ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None };
        Fields.Controls.Add(field); return field;
    }
    protected TextBox[] Translated(string label, LocalText value, bool multiline = false)
    {
        var field = TextField(label, value.Et, multiline);
        field.Tag = value;
        return new[] { field };
    }
    protected static LocalText Read(TextBox[] fields) =>
        (fields[0].Tag as LocalText ?? new()) with { Et = fields[0].Text.Trim() };
    protected NumericUpDown Number(string label, decimal value, decimal maximum, int decimals = 0)
    {
        Fields.Controls.Add(Shop67Ui.Label(label, 10, true));
        var field = new NumericUpDown { Maximum = maximum, DecimalPlaces = decimals, Value = Math.Clamp(value, 0, maximum), Width = 180 };
        Fields.Controls.Add(field); return field;
    }
    protected ComboBox Choice(string label, IEnumerable<string> keys, string value, Func<string, string> display)
    {
        Fields.Controls.Add(Shop67Ui.Label(label, 10, true));
        var field = new ComboBox { Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var key in keys) field.Items.Add(new ChoiceItem(key, display(key)));
        field.SelectedIndex = Enumerable.Range(0, field.Items.Count).FirstOrDefault(i => ((ChoiceItem)field.Items[i]).Key == value);
        Fields.Controls.Add(field); return field;
    }
    protected static string Key(ComboBox field) => (field.SelectedItem as ChoiceItem)?.Key ?? "";
    protected sealed record ChoiceItem(string Key, string Label) { public override string ToString() => Label; }
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
        : base(T.L("Kampaania", "Акция", "Promotion"))
    {
        original = Value = value;
        targetCatalog = products.Select(p => new ShopProduct { Id = p.Id, Name = p.Toodenimetus,
            Details = Shop67Settings.Details(config, p) }).ToArray();
        titles = Translated(T.L("Pealkiri", "Заголовок", "Title"), value.Title);
        descriptions = Translated(T.L("Kirjeldus", "Описание", "Description"), value.Description, true);
        category = Choice(T.L("Kategooria", "Категория", "Category"), CatalogRules.Categories.Prepend(""), value.Category, T.Category);
        category.AccessibleName = "promotion-category";
        percent = Number(T.L("Allahindlus % (0 = ainult pakkumine)", "Скидка % (0 = подборка без скидки)", "Discount % (0 = collection without discount)"), value.Percent, 90, 2);
        position = Number(T.L("Järjekord", "Порядок показа", "Display order"), value.Position, 999);
        Fields.Controls.Add(Shop67Ui.Label(T.L("Algus / lõpp (kohalik aeg)", "Начало / окончание (местное время)", "Start / end (local time)"), 10, true));
        starts = new DateTimePicker { Value = value.Starts.LocalDateTime, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm", Width = 250 };
        ends = new DateTimePicker { Value = value.Ends.LocalDateTime, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm", Width = 250 };
        Fields.Controls.Add(starts); Fields.Controls.Add(ends);
        enabled = new CheckBox { Text = T.L("Avaldatud", "Опубликовано", "Published"), Checked = value.Enabled, Width = 300 }; Fields.Controls.Add(enabled);
        image = TextField(T.L("Bänneri pildi HTTPS-aadress (valikuline)", "HTTPS-адрес баннера (необязательно)", "Banner image HTTPS URL (optional)"), value.Image);
        allProducts = new CheckBox { Text = T.L("Kogu valitud kategooria", "Вся выбранная категория", "Entire selected category"),
            Checked = string.IsNullOrWhiteSpace(value.ProductKeys), Width = 710, Height = 32, AccessibleName = "promotion-all-products" };
        Fields.Controls.Add(allProducts);
        Fields.Controls.Add(Shop67Ui.Label(T.L("Või vali üksikud tooted", "Или выберите отдельные товары", "Or select individual products"), 10, true));
        selectedProducts = new CheckedListBox { Width = 710, Height = 200, CheckOnClick = true, AccessibleName = "promotion-products" };
        targetSummary = Shop67Ui.Label("", 10); targetSummary.MaximumSize = new Size(710, 0);
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
        category.SelectedIndexChanged += (_, _) => Populate(selectedProducts.CheckedItems.Cast<ChoiceItem>().Select(p => p.Key).ToArray());
        allProducts.CheckedChanged += (_, _) => { selectedProducts.Enabled = !allProducts.Checked; UpdateSummary(); };
        selectedProducts.ItemCheck += (_, _) => { if (IsHandleCreated) BeginInvoke(new Action(UpdateSummary)); };
        Populate(selected);
    }
    private void UpdateSummary()
    {
        int count = allProducts.Checked ? selectedProducts.Items.Count : selectedProducts.CheckedItems.Count;
        targetSummary.ForeColor = count == 0 ? Color.Firebrick : ShopStyle.Muted;
        targetSummary.Text = count == 0 ? T.L("Vali tooted või kogu kategooria. Varasem valik võib olla peidetud või teises kategoorias.", "Выберите товары или всю категорию. Ранее выбранные позиции могли быть скрыты или относиться к другой категории.", "Select products or the entire category. Previous selections may be hidden or in another category.")
            : string.Format(T.L("Kampaania kehtib {0} tootele.", "Акция применяется к товарам: {0}.", "Promotion applies to {0} products."), count);
    }

    protected override void Save()
    {
        if (image.Text.Length > 0 && (!Uri.TryCreate(image.Text, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            throw new InvalidOperationException(T.L("Kasuta HTTPS-aadressi.", "Укажите HTTPS-адрес.", "Use an HTTPS URL."));
        if (!allProducts.Checked && selectedProducts.CheckedItems.Count == 0)
            throw new InvalidOperationException(T.L("Vali vähemalt üks toode või kogu kategooria.", "Выберите хотя бы один товар или включите всю категорию.", "Select at least one product or enable the entire category."));
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
    public ProductEditorForm(int id) : base(T.L("Toote andmed", "Данные товара", "Product details"))
    {
        productId = id;
        using var db = new AppDbContext(); var product = db.Toodetabel.Find(id) ?? throw new InvalidOperationException("Product not found");
        original = Shop67Settings.Details(Shop67Settings.Load(db), product);
        names = Translated(T.L("Nimetus", "Название", "Name"), original.Name.Et.Length == 0 ? new(product.Toodenimetus, product.Toodenimetus, product.Toodenimetus) : original.Name);
        descriptions = Translated(T.L("Kirjeldus", "Описание", "Description"), original.Description, true);
        category = Choice(T.L("Kategooria", "Категория", "Category"), CatalogRules.Categories, original.Category, T.Category);
        kind = Choice(T.Spec("kind"), new[] { "phone", "laptop", "desktop", "mouse", "keyboard", "headphones" }, original.Kind, T.Value);
        brand = TextField(T.Spec("brand"), original.Brand);
        price = Number(T.L("Tavahind (€)", "Обычная цена (€)", "Regular price (€)"), (decimal)product.Hind, 1000000, 2);
        stock = Number(T.L("Laoseis", "Остаток", "Stock"), product.Kogus, 1000000);
        visible = new CheckBox { Text = T.L("Näita poes", "Показывать в магазине", "Show in shop"), Checked = original.Visible, Width = 400 }; Fields.Controls.Add(visible);
        image = TextField(T.L("Pildi HTTPS-aadress või kohalik fail", "HTTPS-адрес фото или локальный файл", "Image HTTPS URL or local file"), product.Pilt ?? "");
        Fields.Controls.Add(Shop67Ui.Label(T.L("Omadused · tühi väärtus = teadmata", "Характеристики · пустое значение = неизвестно", "Specifications · empty value = unknown"), 11, true));
        specs = new DataGridView { Width = 710, Height = 280, AllowUserToAddRows = true, AllowUserToDeleteRows = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
        var keys = new DataGridViewComboBoxColumn { Name = "Key", HeaderText = T.L("Omadus", "Характеристика", "Specification"), FlatStyle = FlatStyle.Flat };
        keys.DataSource = CatalogRules.Categories.SelectMany(CatalogRules.Facets).Concat(original.Specs.Keys).Distinct()
            .Where(k => k != "brand" && k != "kind").Select(k => new ChoiceItem(k, T.Spec(k))).ToList();
        keys.ValueMember = "Key"; keys.DisplayMember = "Label";
        specs.Columns.Add(keys); specs.Columns.Add("Value", T.L("Väärtus", "Значение", "Value"));
        foreach (var pair in original.Specs) specs.Rows.Add(pair.Key, pair.Value);
        specs.CellFormatting += (_, e) => { if (e.ColumnIndex == 0 && e.Value is string key) { e.Value = T.Spec(key); e.FormattingApplied = true; } };
        specs.DataError += (_, e) => { e.ThrowException = false; };
        Fields.Controls.Add(specs);
    }
    protected override void Save()
    {
        specs.EndEdit();
        var translated = Read(names);
        if (string.IsNullOrWhiteSpace(translated.Et))
            throw new InvalidOperationException(T.L("Sisesta toote nimetus.", "Введите название на трёх языках.", "Enter a name in all three languages."));
        var values = new Dictionary<string, string>();
        foreach (DataGridViewRow row in specs.Rows)
        {
            if (row.IsNewRow) continue;
            string key = Convert.ToString(row.Cells[0].Value)?.Trim() ?? "";
            string value = Convert.ToString(row.Cells[1].Value)?.Trim() ?? "";
            if (value.Length == 0) continue;
            if (key.Length == 0 || !values.TryAdd(key, value)) throw new InvalidOperationException(T.L("Omadus puudub või kordub.", "Характеристика не выбрана или повторяется.", "A specification is missing or duplicated."));
        }
        string selectedCategory = Key(category), selectedKind = Key(kind);
        if ((selectedCategory == "phones" && selectedKind != "phone") || (selectedCategory == "computers" && selectedKind is not ("laptop" or "desktop")) || (selectedCategory == "accessories" && selectedKind is not ("mouse" or "keyboard" or "headphones")))
            throw new InvalidOperationException(T.L("Seadme tüüp ei sobi kategooriaga.", "Тип устройства не соответствует категории.", "The device type does not match the category."));
        using var db = new AppDbContext(); using var transaction = db.Database.BeginTransaction();
        var product = db.Toodetabel.Find(productId) ?? throw new InvalidOperationException("Product not found");
        var config = Shop67Settings.Load(db);
        var detail = original with { Name = translated, Description = Read(descriptions), Brand = brand.Text.Trim(), Category = selectedCategory,
            Kind = selectedKind, Specs = values, Visible = visible.Checked, SourceKey = original.SourceKey.Length == 0 ? "local:" + productId : original.SourceKey };
        if (!CatalogRules.IsTechnical(new ShopProduct { Name = translated.Et, Details = detail }))
            throw new InvalidOperationException(T.L("Kontrolli tegelikku tootemudelit, kaubamärki ja kategooriat.", "Укажите реальную модель, производителя и подходящую категорию.", "Enter a real product model, manufacturer and matching category."));
        if (detail.Visible && string.IsNullOrWhiteSpace(image.Text))
            throw new InvalidOperationException(T.L("Nähtaval tootel peab olema pilt.", "Для товара в витрине нужно изображение.", "A visible product must have an image."));
        var dbCategory = db.Kategooriatabel.FirstOrDefault(c => c.nimetus == selectedCategory);
        if (dbCategory == null) { dbCategory = new Kategooria { nimetus = selectedCategory }; db.Kategooriatabel.Add(dbCategory); }
        product.Kategooria = dbCategory; product.Toodenimetus = translated.Et; product.Hind = (float)price.Value;
        product.Kogus = (int)stock.Value; product.Pilt = image.Text.Trim(); config.Products[productId] = detail;
        Shop67Settings.Save(db, config); transaction.Commit();
    }
}
