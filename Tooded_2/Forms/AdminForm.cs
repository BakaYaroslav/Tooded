using Microsoft.EntityFrameworkCore;

namespace Tooded_2;
internal sealed class AdminForm : LocalizedForm
{
    private DataGridView productGrid = null!, offerGrid = null!;
    private List<Toode> products = new();
    private ShopConfiguration configuration = new();
    private readonly Label salesSummary = ShopUi.Label(() => "");
    public AdminForm()
    {
        LanguageManager.Bind(this, "Text", () => "shoping67 · " + UiText.Get("Management"));
        Size = new Size(1100, 750);
        MinimumSize = new Size(950, 600);
        Font = new Font("Segoe UI", 10);
        StartPosition = FormStartPosition.CenterParent;
        Build();
        Reload();
    }

    private static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        AutoGenerateColumns = true
    };
    private void Build()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(10, 6, 0, 0),
            WrapContents = false
        };
        toolbar.Controls.Add(ShopUi.Button(() => UiText.Get("OpenShop"), OpenShop));
        toolbar.Controls.Add(ShopUi.Button(() => UiText.Get("EmailSettings"), OpenEmailSettings));
        var language = LanguageManager.CreateSelector();
        language.Margin = new Padding(12, 6, 0, 0);
        var languageBar = new Panel
        {
            Dock = DockStyle.Right,
            Width = 130
        };
        language.Dock = DockStyle.Top;
        languageBar.Controls.Add(language);
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52
        };
        header.Controls.Add(toolbar);
        header.Controls.Add(languageBar);
        salesSummary.Dock = DockStyle.Bottom;
        salesSummary.Height = 34;
        salesSummary.AutoSize = false;
        salesSummary.Padding = new Padding(12, 6, 0, 0);
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };
        var productPage = new TabPage(UiText.Get("ProductsAndSpecifications"));
        var offerPage = new TabPage(UiText.Get("OffersAndPromotions"));
        productGrid = Grid();
        offerGrid = Grid();
        productGrid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex >= 0 && productGrid.Columns[e.ColumnIndex].Name == "Category" && e.Value is string key)
            {
                e.Value = UiText.Category(key);
                e.FormattingApplied = true;
            }
        };
        productPage.Name = "productPage";
        offerPage.Name = "offerPage";
        productPage.Controls.Add(productGrid);
        offerPage.Controls.Add(offerGrid);
        var productActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(10)
        };
        productActions.Controls.Add(ShopUi.Button(() => UiText.Get("AddProduct"), AddProduct, true));
        var edit = ShopUi.Button(() => UiText.Get("EditSelectedProduct"), EditProduct, true);
        edit.Width = 220;
        productActions.Controls.Add(edit);
        var delete = ShopUi.Button(() => UiText.Get("DeleteProduct"), DeleteProduct);
        delete.AccessibleName = "delete-product";
        delete.ForeColor = Color.Firebrick;
        productActions.Controls.Add(delete);
        productActions.Controls.Add(ShopUi.Button(() => UiText.Get("Refresh"), Reload));
        productPage.Controls.Add(productActions);
        var offerActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(10)
        };
        offerActions.Controls.Add(ShopUi.Button(() => UiText.Get("AddPromotion"), () => EditOffer(null), true));
        offerActions.Controls.Add(ShopUi.Button(() => UiText.Get("Refresh"), () => EditOffer(SelectedOffer())));
        offerActions.Controls.Add(ShopUi.Button(() => UiText.Get("Delete"), DeleteOffer));
        offerPage.Controls.Add(offerActions);
        productGrid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                EditProduct();
        };
        offerGrid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                EditOffer(SelectedOffer());
        };
        tabs.TabPages.Add(productPage);
        tabs.TabPages.Add(offerPage);
        Controls.Add(tabs);
        Controls.Add(salesSummary);
        Controls.Add(header);
    }

    private void Reload()
    {
        using var db = new AppDbContext();
        configuration = ShopSettingsStore.Load(db);
        products = db.Toodetabel.AsNoTracking().OrderBy(p => p.Id).ToList();
        productGrid.DataSource = products.Select(p => new { Id = p.Id, Name = p.Toodenimetus, Category = ShopSettingsStore.Details(configuration, p).Category, Price = p.Hind, Stock = p.Kogus, Visible = ShopSettingsStore.Details(configuration, p).Visible }).ToList();
        offerGrid.DataSource = configuration.Promotions.OrderBy(p => p.Position).Select(p => new { Id = p.Id, Title = p.Title.Value, Discount = p.Percent, Starts = p.Starts.LocalDateTime, Ends = p.Ends.LocalDateTime, Active = p.Active(DateTimeOffset.UtcNow), Position = p.Position }).ToList();
        if (offerGrid.Columns.Contains("Id"))
            offerGrid.Columns["Id"]!.Visible = false;
        int quantity = db.Muugitabel.Sum(sale => (int? )sale.Kogus) ?? 0;
        decimal revenue = db.Muugitabel.Sum(sale => (decimal? )sale.Summa) ?? 0;
        LanguageManager.Bind(salesSummary, "Text", () => UiText.Format("Sold0Revenue1", quantity, UiText.Money(revenue)));
        Headers(productGrid);
        Headers(offerGrid);
    }

    private static void Headers(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns)
            column.HeaderText = column.Name switch
            {
                "Name" => UiText.Get("Name"),
                "Category" => UiText.Get("Category"),
                "Price" => UiText.Get("Price"),
                "Stock" => UiText.Get("Stock"),
                "Visible" => UiText.Get("Visible"),
                "Title" => UiText.Get("Title"),
                "Discount" => UiText.Get("Discount"),
                "Starts" => UiText.Get("Starts"),
                "Ends" => UiText.Get("EndDate"),
                "Active" => UiText.Get("Active"),
                "Position" => UiText.Get("DisplayOrder"),
                _ => column.Name
            };
    }

    protected override void RefreshLocalizedValues()
    {
        Headers(productGrid);
        Headers(offerGrid);
        productGrid.Invalidate();
        offerGrid.Invalidate();
    }

    private Promotion? SelectedOffer() => offerGrid.CurrentRow == null ? null : configuration.Promotions.FirstOrDefault(p => p.Id == Convert.ToString(offerGrid.CurrentRow.Cells["Id"].Value));
    private void AddProduct()
    {
        using var editor = new ProductEditorForm();
        if (editor.ShowDialog(this) == DialogResult.OK)
            Reload();
    }

    private void OpenShop()
    {
        using var shop = new StoreForm();
        shop.ShowDialog(this);
        Reload();
    }

    private void OpenEmailSettings()
    {
        using var settings = new MailSettingsForm();
        settings.ShowDialog(this);
    }

    private void EditProduct()
    {
        if (productGrid.CurrentRow == null)
            return;
        int id = Convert.ToInt32(productGrid.CurrentRow.Cells["Id"].Value);
        using var editor = new ProductEditorForm(id);
        if (editor.ShowDialog(this) == DialogResult.OK)
            Reload();
    }

    private void DeleteProduct()
    {
        if (productGrid.CurrentRow == null)
        {
            MessageBox.Show(this, UiText.Get("SelectProduct"), "shoping67");
            return;
        }

        int id = Convert.ToInt32(productGrid.CurrentRow.Cells["Id"].Value);
        string name = Convert.ToString(productGrid.CurrentRow.Cells["Name"].Value) ?? "";
        if (MessageBox.Show(this, UiText.Format("DeleteProductConfirmation", name), UiText.Get("DeleteProduct"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            return;
        try
        {
            if (!ShopSettingsStore.DeleteProduct(id))
                MessageBox.Show(this, UiText.Get("TheProductHasAlreadyBeenDeleted"), "shoping67");
            Reload();
        }
        catch (Exception)
        {
            MessageBox.Show(this, UiText.Get("ProductDeleteFailed"), "shoping67", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EditOffer(Promotion? offer)
    {
        using var editor = new PromotionEditorForm(offer ?? new Promotion(), products, configuration);
        if (editor.ShowDialog(this) != DialogResult.OK)
            return;
        using var db = new AppDbContext();
        var current = ShopSettingsStore.Load(db);
        var catalog = db.Toodetabel.AsNoTracking().ToList().Select(p => new ShopProduct { Id = p.Id, Name = p.Toodenimetus, Details = ShopSettingsStore.Details(current, p) }).ToArray();
        try
        {
            PromotionTargets.Validate(editor.Value, catalog);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "shoping67");
            return;
        }

        current.Promotions.RemoveAll(p => p.Id == editor.Value.Id);
        current.Promotions.Add(editor.Value);
        current.Initialized = true;
        ShopSettingsStore.Save(db, current);
        Reload();
    }

    private void DeleteOffer()
    {
        var offer = SelectedOffer();
        if (offer == null)
            return;
        if (MessageBox.Show(this, UiText.Get("DeleteThisPromotion"), "shoping67", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;
        using var db = new AppDbContext();
        var current = ShopSettingsStore.Load(db);
        current.Promotions.RemoveAll(p => p.Id == offer.Id);
        ShopSettingsStore.Save(db, current);
        Reload();
    }
}
