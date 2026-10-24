#nullable enable
using Tooded_2.Shop;

namespace Tooded_2;

partial class PoodForm
{
    private System.ComponentModel.IContainer? components;
    private TextBox search = null!;
    private ListBox categories = null!;
    private ComboBox sort = null!;
    private CheckBox inStock = null!;
    private FlowLayoutPanel productsPanel = null!;
    private Label results = null!;
    private Label status = null!;
    private Button basket = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) { components?.Dispose(); store.Dispose(); }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        Font = new Font("Segoe UI", 10); Text = "Tooded · Pood";
        ClientSize = new Size(1260, 810); MinimumSize = new Size(1050, 670);
        StartPosition = FormStartPosition.CenterScreen; BackColor = Color.FromArgb(245, 247, 249);
        AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = ShopStyle.Navy, Padding = new Padding(24, 16, 24, 12), Margin = Padding.Empty };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        var brand = ShopStyle.Label("tooded.", 27, true); brand.ForeColor = Color.White;
        header.Controls.Add(brand, 0, 0);
        var tagline = ShopStyle.Label("Sinu järgmine hea leid.", 12); tagline.ForeColor = Color.White; tagline.Anchor = AnchorStyles.Left;
        header.Controls.Add(tagline, 1, 0);
        basket = ShopStyle.Button("Ostukorv · 0", true); basket.Dock = DockStyle.Fill;
        header.Controls.Add(basket, 2, 0); root.Controls.Add(header, 0, 0);
        var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(24, 12, 24, 8), BackColor = Color.White, Margin = Padding.Empty };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        toolbar.Controls.Add(ShopStyle.Label("Otsi tooteid", 10, true), 0, 0);
        search = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Toote nimi või kategooria…", Margin = new Padding(0, 0, 20, 0), AccessibleName = "Otsi tooteid" };
        toolbar.Controls.Add(search, 1, 0);
        var refresh = ShopStyle.Button("Uuenda"); refresh.Dock = DockStyle.Fill; refresh.Click += (_, _) => Reload();
        toolbar.Controls.Add(refresh, 2, 0); root.Controls.Add(toolbar, 0, 1);
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(24, 20, 24, 0) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210)); body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var sidebar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(0, 0, 20, 0) };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        sidebar.Controls.Add(ShopStyle.Label("Kategooriad", 13, true), 0, 0);
        categories = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = BackColor, IntegralHeight = false, ItemHeight = 30, AccessibleName = "Kategooriad" };
        sidebar.Controls.Add(categories, 0, 1);
        inStock = new CheckBox { Text = "Ainult laos olevad", Dock = DockStyle.Fill, AutoSize = true }; sidebar.Controls.Add(inStock, 0, 2);
        var reset = ShopStyle.Button("Puhasta filtrid"); reset.Dock = DockStyle.Fill;
        reset.Click += (_, _) => { search.Clear(); categories.SelectedIndex = 0; inStock.Checked = false; sort.SelectedIndex = 0; };
        sidebar.Controls.Add(reset, 0, 3);
        var demoNote = ShopStyle.Label("Õppepood\nAinult demomakse.\nRaha ei võeta, kaupu ei saadeta.", 9); demoNote.Dock = DockStyle.Fill; demoNote.AutoSize = false; demoNote.ForeColor = ShopStyle.Muted;
        sidebar.Controls.Add(demoNote, 0, 4); body.Controls.Add(sidebar, 0, 0);
        var catalog = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        catalog.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); catalog.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var heading = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); heading.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225));
        results = ShopStyle.Label("Tooted", 19, true); heading.Controls.Add(results, 0, 0);
        sort = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, AccessibleName = "Sorteerimine" };
        sort.Items.AddRange(new object[] { "Soovitatud järjestus", "Hind: odavamad enne", "Hind: kallimad enne", "Nimi: A–Z" }); sort.SelectedIndex = 0;
        heading.Controls.Add(sort, 1, 0); catalog.Controls.Add(heading, 0, 0);
        productsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 0, 8, 0) };
        catalog.Controls.Add(productsPanel, 0, 1); body.Controls.Add(catalog, 1, 0); root.Controls.Add(body, 0, 2);
        status = ShopStyle.Label("", 9); status.Dock = DockStyle.Fill; status.AutoSize = false; status.Padding = new Padding(24, 12, 0, 0); status.ForeColor = ShopStyle.Muted;
        root.Controls.Add(status, 0, 3); Controls.Add(root); ResumeLayout(false);
    }
}
