using Microsoft.EntityFrameworkCore;

namespace Tooded_2;
internal sealed class PromotionEditorForm : EditorForm
{
    private readonly Promotion original;
    private readonly TextBox titleField, descriptionField;
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

    public PromotionEditorForm(Promotion value, IReadOnlyList<Toode> products, ShopConfiguration config) : base(() => UiText.Get("Promotion"))
    {
        original = Value = value;
        targetCatalog = products.Select(p => new ShopProduct { Id = p.Id, Name = p.Toodenimetus, Details = ShopSettingsStore.Details(config, p) }).ToArray();
        titleField = ContentField(() => UiText.Get("Title"), value.Title);
        descriptionField = ContentField(() => UiText.Get("Description"), value.Description, true);
        category = Choice(() => UiText.Get("Category"), CatalogRules.Categories.Prepend(""), value.Category, UiText.Category);
        category.AccessibleName = "promotion-category";
        percent = Number(() => UiText.Get("Discount0CollectionWithoutDiscount"), value.Percent, 90, 2);
        position = Number(() => UiText.Get("DisplayOrder"), value.Position, 999);
        Fields.Controls.Add(ShopUi.Label(() => UiText.Get("StartEndLocalTime"), 10, true));
        starts = new DateTimePicker
        {
            Value = value.Starts.LocalDateTime,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd.MM.yyyy HH:mm",
            Width = 250
        };
        ends = new DateTimePicker
        {
            Value = value.Ends.LocalDateTime,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd.MM.yyyy HH:mm",
            Width = 250
        };
        Fields.Controls.Add(starts);
        Fields.Controls.Add(ends);
        enabled = LanguageManager.Bind(new CheckBox { Checked = value.Enabled, Width = 300 }, "Text", () => UiText.Get("Published"));
        Fields.Controls.Add(enabled);
        image = TextField(() => UiText.Get("BannerImageHTTPSURLOptional"), value.Image);
        allProducts = LanguageManager.Bind(new CheckBox { Checked = string.IsNullOrWhiteSpace(value.ProductKeys), Width = 710, Height = 32, AccessibleName = "promotion-all-products" }, "Text", () => UiText.Get("EntireSelectedCategory"));
        Fields.Controls.Add(allProducts);
        Fields.Controls.Add(ShopUi.Label(() => UiText.Get("OrSelectIndividualProducts"), 10, true));
        selectedProducts = new CheckedListBox
        {
            Width = 710,
            Height = 200,
            CheckOnClick = true,
            AccessibleName = "promotion-products"
        };
        targetSummary = ShopUi.Label(() => "", 10);
        targetSummary.MaximumSize = new Size(710, 0);
        targetSummary.AccessibleName = "promotion-target-summary";
        Fields.Controls.Add(selectedProducts);
        Fields.Controls.Add(targetSummary);
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

        category.SelectedIndexChanged += (_, _) =>
        {
            if (!LanguageManager.IsApplying)
                Populate(selectedProducts.CheckedItems.Cast<ChoiceItem>().Select(p => p.Key).ToArray());
        };
        allProducts.CheckedChanged += (_, _) =>
        {
            selectedProducts.Enabled = !allProducts.Checked;
            UpdateSummary();
        };
        selectedProducts.ItemCheck += (_, _) =>
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(UpdateSummary));
        };
        Populate(selected);
    }

    private void UpdateSummary()
    {
        int count = allProducts.Checked ? selectedProducts.Items.Count : selectedProducts.CheckedItems.Count;
        targetSummary.ForeColor = count == 0 ? Color.Firebrick : ShopUi.Muted;
        LanguageManager.Bind(targetSummary, "Text", () => count == 0 ? UiText.Get("PromotionSelectionHelp") : string.Format(UiText.Get("PromotionAppliesTo0Products"), count));
    }

    protected override void Save()
    {
        if (image.Text.Length > 0 && (!Uri.TryCreate(image.Text, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            throw new InvalidOperationException(UiText.Get("UseAnHTTPSURL"));
        if (!allProducts.Checked && selectedProducts.CheckedItems.Count == 0)
            throw new InvalidOperationException(UiText.Get("PromotionSelectionRequired"));
        Value = original with
        {
            Title = Read(titleField),
            Description = Read(descriptionField),
            Category = Key(category),
            Percent = percent.Value,
            Position = (int)position.Value,
            Starts = new DateTimeOffset(starts.Value),
            Ends = new DateTimeOffset(ends.Value),
            Enabled = enabled.Checked,
            Image = image.Text.Trim(),
            ProductKeys = allProducts.Checked ? "" : string.Join(",", selectedProducts.CheckedItems.Cast<ChoiceItem>().Select(i => i.Key))
        };
        PromotionTargets.Validate(Value, targetCatalog);
    }
}
