using Microsoft.EntityFrameworkCore;

namespace Tooded_2;
internal sealed class ProductEditorForm : EditorForm
{
    private readonly int? productId;
    private readonly ProductDetails original;
    private readonly TextBox nameField, descriptionField;
    private readonly TextBox brand, image;
    private readonly ComboBox category, kind;
    private readonly NumericUpDown price, stock;
    private readonly CheckBox visible;
    private readonly DataGridView specs;
    public ProductEditorForm(int? id = null) : base(() => UiText.Get("ProductDetails"))
    {
        productId = id;
        using var db = new AppDbContext();
        var product = id.HasValue ? db.Toodetabel.Find(id.Value) : new Toode
        {
            Kogus = 1
        };
        if (product == null)
            throw new InvalidOperationException(UiText.Get("ProductNotFound"));
        original = id.HasValue ? ShopSettingsStore.Details(ShopSettingsStore.Load(db), product) : new ProductDetails
        {
            Category = "phones",
            Kind = "phone"
        };
        nameField = ContentField(() => UiText.Get("Name"), original.Name.Et.Length == 0 ? new(product.Toodenimetus ?? "", product.Toodenimetus ?? "") : original.Name);
        descriptionField = ContentField(() => UiText.Get("Description"), original.Description, true);
        category = Choice(() => UiText.Get("Category"), CatalogRules.Categories, original.Category, UiText.Category);
        kind = Choice(() => UiText.Spec("kind"), new[] { "phone", "laptop", "desktop", "mouse", "keyboard", "headphones" }, original.Kind, UiText.Value);
        brand = TextField(() => UiText.Spec("brand"), original.Brand);
        price = Number(() => UiText.Get("RegularPrice"), (decimal)product.Hind, 1000000, 2);
        stock = Number(() => UiText.Get("Stock"), product.Kogus, 1000000);
        visible = LanguageManager.Bind(new CheckBox { Checked = original.Visible, Width = 400 }, "Text", () => UiText.Get("ShowInShop"));
        Fields.Controls.Add(visible);
        image = TextField(() => UiText.Get("ImageHTTPSURLOrLocalFile"), product.Pilt ?? "");
        var chooseImage = ShopUi.Button(() => UiText.Get("ChooseImage"), () =>
        {
            using var picker = new OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.bmp"
            };
            if (picker.ShowDialog(this) == DialogResult.OK)
                image.Text = picker.FileName;
        });
        Fields.Controls.Add(chooseImage);
        Fields.Controls.Add(ShopUi.Label(() => UiText.Get("SpecificationsEmptyValueUnknown"), 11, true));
        specs = new DataGridView
        {
            Width = 710,
            Height = 280,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false
        };
        var keys = new DataGridViewComboBoxColumn
        {
            Name = "Key",
            HeaderText = UiText.Get("Specification"),
            FlatStyle = FlatStyle.Flat
        };
        keys.DataSource = CatalogRules.Categories.SelectMany(CatalogRules.Facets).Concat(original.Specs.Keys).Distinct().Where(k => k != "brand" && k != "kind").Select(k => new ChoiceItem(k, "") { Display = () => UiText.Spec(k) }).ToList();
        keys.ValueMember = "Key";
        keys.DisplayMember = "DisplayLabel";
        specs.Columns.Add(keys);
        specs.Columns.Add("Value", UiText.Get("Value"));
        LanguageManager.OnRefresh(specs, () =>
        {
            specs.Columns["Key"]!.HeaderText = UiText.Get("Specification");
            specs.Columns["Value"]!.HeaderText = UiText.Get("Value");
            specs.Invalidate();
        });
        foreach (var pair in original.Specs)
            specs.Rows.Add(pair.Key, pair.Value);
        specs.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex == 0 && e.Value is string key)
            {
                e.Value = UiText.Spec(key);
                e.FormattingApplied = true;
            }
        };
        specs.DataError += (_, e) =>
        {
            e.ThrowException = false;
        };
        Fields.Controls.Add(specs);
    }

    protected override void Save()
    {
        specs.EndEdit();
        var translated = Read(nameField);
        if (string.IsNullOrWhiteSpace(translated.Et))
            throw new InvalidOperationException(UiText.Get("EnterAProductName"));
        var values = new Dictionary<string, string>();
        foreach (DataGridViewRow row in specs.Rows)
        {
            if (row.IsNewRow)
                continue;
            string key = Convert.ToString(row.Cells[0].Value)?.Trim() ?? "";
            string value = Convert.ToString(row.Cells[1].Value)?.Trim() ?? "";
            if (value.Length == 0)
                continue;
            if (key.Length == 0 || !values.TryAdd(key, value))
                throw new InvalidOperationException(UiText.Get("ASpecificationIsMissingOrDuplicated"));
        }

        string selectedCategory = Key(category), selectedKind = Key(kind);
        if ((selectedCategory == "phones" && selectedKind != "phone") || (selectedCategory == "computers" && selectedKind is not ("laptop" or "desktop")) || (selectedCategory == "accessories" && selectedKind is not ("mouse" or "keyboard" or "headphones")))
            throw new InvalidOperationException(UiText.Get("ProductCategoryMismatch"));
        var details = original with
        {
            Name = translated,
            Description = Read(descriptionField),
            Brand = brand.Text.Trim(),
            Category = selectedCategory,
            Kind = selectedKind,
            Specs = values,
            Visible = visible.Checked
        };
        ProductService.Save(productId, details, price.Value, (int)stock.Value, image.Text);
    }
}
