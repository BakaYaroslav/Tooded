using Tooded_2.Shop;

namespace Tooded_2;

public partial class PoodForm : Form
{
    private readonly IShopStore store;
    private readonly ShoppingCart cart = new();
    private IReadOnlyList<ShopProduct> products = Array.Empty<ShopProduct>();
    private IReadOnlyList<Promotion> promotions = Array.Empty<Promotion>();
    private readonly Dictionary<string, HashSet<string>> facets = new();
    private readonly System.Windows.Forms.Timer clock = new() { Interval = 30000 };
    private string activeSignature = "";
    private string category = "", query = "";
    private string? selectedOffer;
    private bool home = true, inStock, loading;
    private int sortOrder;
    private decimal minimum, maximum = 1000000;
    private FlowLayoutPanel resultsPanel = null!, sidebar = null!;
    private TableLayoutPanel body = null!;
    private Label status = null!, heading = null!;
    private Button basket = null!;

    public PoodForm() : this(new DatabaseShopStore()) { }
    public PoodForm(IShopStore store)
    {
        this.store = store;
        Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        Text = "shoping67"; Size = new Size(1360, 900); MinimumSize = new Size(1060, 700);
        StartPosition = FormStartPosition.CenterScreen; BackColor = Color.FromArgb(244, 246, 248);
        Build(); Shown += (_, _) => Reload();
        clock.Tick += (_, _) =>
        {
            var signature = string.Join(",", promotions.Where(p => p.Active(DateTimeOffset.UtcNow)).Select(p => p.Id));
            if (signature == activeSignature || loading) return;
            activeSignature = signature;
            products = products.Select(p => PromotionEngine.Apply(p, promotions, DateTimeOffset.UtcNow)).ToArray();
            Render();
        };
        clock.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { clock.Dispose(); store.Dispose(); }
        base.Dispose(disposing);
    }

    private void Build()
    {
        SuspendLayout(); ShopStyle.Clear(this);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        var header = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = ShopStyle.Navy, ColumnCount = 3, Padding = new Padding(18, 10, 18, 10), Margin = Padding.Empty };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 218)); header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225));
        var logo = new Shop67Logo { Dock = DockStyle.Fill, Cursor = Cursors.Hand }; logo.Click += (_, _) => Navigate("", true); header.Controls.Add(logo, 0, 0);
        var search = new TextBox { Text = query, Dock = DockStyle.Fill, PlaceholderText = T.L("Otsi toodet, mudelit või omadust…", "Название, модель или характеристика…", "Search products, models or specifications…"), Margin = new Padding(8, 16, 18, 4), AccessibleName = T.L("Otsing", "Поиск", "Search") };
        search.TextChanged += (_, _) => { query = search.Text; home = false; Render(); }; header.Controls.Add(search, 1, 0);
        basket = Shop67Ui.Button("", OpenCart, true); basket.Dock = DockStyle.Fill; header.Controls.Add(basket, 2, 0);
        root.Controls.Add(header, 0, 0);
        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20, 5, 10, 0), WrapContents = false };
        var homeButton = Shop67Ui.Button(T.L("Avaleht ja pakkumised", "Главная и акции", "Home and offers"), () => Navigate("", true)); homeButton.Width = 200; nav.Controls.Add(homeButton);
        foreach (string key in CatalogRules.Categories)
        { var button = Shop67Ui.Button(T.Category(key), () => Navigate(key, false)); button.Width = key == "phones" ? 135 : 240; nav.Controls.Add(button); }
        var refresh = Shop67Ui.Button(T.L("Uuenda", "Обновить", "Refresh"), Reload); refresh.Width = 120; nav.Controls.Add(refresh);
        root.Controls.Add(nav, 0, 1);
        body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(20, 16, 12, 0) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245)); body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sidebar = ShopStyle.Stack(); sidebar.Padding = new Padding(0, 0, 18, 0); body.Controls.Add(sidebar, 0, 0);
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var titlebar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        titlebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); titlebar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        heading = Shop67Ui.Label("", 18, true); titlebar.Controls.Add(heading, 0, 0);
        var sort = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
        sort.Items.AddRange(new object[] { T.L("Soovitatud", "Рекомендуемые", "Recommended"), T.L("Hind: odavamad enne", "Цена по возрастанию", "Price: low to high"), T.L("Hind: kallimad enne", "Цена по убыванию", "Price: high to low"), T.L("Nimi: A–Z", "Название: А–Я", "Name: A–Z") });
        sort.SelectedIndex = sortOrder; sort.SelectedIndexChanged += (_, _) => { sortOrder = sort.SelectedIndex; RenderProducts(); }; titlebar.Controls.Add(sort, 1, 0);
        content.Controls.Add(titlebar, 0, 0);
        resultsPanel = ShopStyle.Stack(); resultsPanel.Padding = new Padding(0, 0, 10, 0); resultsPanel.SizeChanged += (_, _) => ResizeCards(); content.Controls.Add(resultsPanel, 0, 1);
        body.Controls.Add(content, 1, 0); root.Controls.Add(body, 0, 2);
        status = Shop67Ui.Label("", 9); status.AutoSize = false; status.Dock = DockStyle.Fill; status.Padding = new Padding(20, 8, 0, 0); root.Controls.Add(status, 0, 3);
        Controls.Add(root); Render(); UpdateCart(); ResumeLayout(true);
    }

    private void Navigate(string key, bool landing)
    { category = key; home = landing; selectedOffer = null; facets.Clear(); Render(); }

    private async void Reload()
    {
        if (loading || IsDisposed) return;
        loading = true; status.Text = T.L("Laadin veebikataloogi…", "Загружаю интернет-каталог…", "Loading online catalog…");
        try
        {
            var loaded = await Task.Run(store.GetProducts);
            if (IsDisposed) return;
            products = loaded; promotions = store.GetPromotions();
            activeSignature = string.Join(",", promotions.Where(p => p.Active(DateTimeOffset.UtcNow)).Select(p => p.Id));
        }
        catch (Exception ex)
        {
            if (!IsDisposed) MessageBox.Show(this, T.L("Kataloogi ei saanud laadida. ", "Не удалось загрузить каталог. ", "Unable to load catalog. ") + ex.Message, "shoping67");
        }
        finally { loading = false; if (!IsDisposed) Render(); }
    }

    private void Render()
    {
        if (resultsPanel == null) return;
        sidebar.Visible = !home; body.ColumnStyles[0].Width = home ? 0 : 245;
        BuildSidebar(); RenderProducts();
        if (!loading) status.Text = T.Legacy(T.Canonical(store.CatalogStatus)) + " · " + T.L("Õppepood · Tegelikud tootemudelid, näitlik hind ja laoseis.", "Учебный магазин · Реальные модели, демонстрационные цены и остатки.", "Teaching shop · Real models, illustrative prices and inventory.");
    }

    private void BuildSidebar()
    {
        ShopStyle.Clear(sidebar);
        if (home) return;
        sidebar.Controls.Add(Shop67Ui.Label(T.L("Filtrid", "Фильтры", "Filters"), 15, true));
        var all = Shop67Ui.Button(T.Category(""), () => Navigate("", false)); all.Width = 210; sidebar.Controls.Add(all);
        foreach (string key in CatalogRules.Categories)
        { var button = Shop67Ui.Button(T.Category(key), () => Navigate(key, false)); button.Width = 210; sidebar.Controls.Add(button); }
        sidebar.Controls.Add(Shop67Ui.Label(T.L("Hind (€): alates / kuni", "Цена (€): от / до", "Price (€): from / to"), 10, true));
        var prices = new FlowLayoutPanel { Width = 215, Height = 42, WrapContents = false };
        var min = new NumericUpDown { DecimalPlaces = 2, Maximum = 1000000, Value = minimum, Width = 98 };
        var max = new NumericUpDown { DecimalPlaces = 2, Maximum = 1000000, Value = maximum, Width = 98 };
        min.ValueChanged += (_, _) => { minimum = min.Value; RenderProducts(); }; max.ValueChanged += (_, _) => { maximum = max.Value; RenderProducts(); };
        prices.Controls.Add(min); prices.Controls.Add(max); sidebar.Controls.Add(prices);
        var available = new CheckBox { Text = T.L("Ainult laos", "Только в наличии", "In stock only"), Checked = inStock, Width = 210, Height = 32 };
        available.CheckedChanged += (_, _) => { inStock = available.Checked; RenderProducts(); }; sidebar.Controls.Add(available);
        var population = products.Where(p => p.Details.Visible && CatalogRules.IsTechnical(p) && (category.Length == 0 || p.Details.Category == category)).ToArray();
        foreach (string facet in CatalogRules.Facets(category))
        {
            var values = population.Select(p => CatalogRules.FacetValue(p, facet)).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().OrderBy(v => v).ToArray();
            if (values.Length == 0) continue;
            sidebar.Controls.Add(Shop67Ui.Label(T.Spec(facet), 11, true));
            var list = new CheckedListBox { Width = 210, Height = Math.Min(170, values.Length * 24 + 8), CheckOnClick = true, BorderStyle = BorderStyle.None, BackColor = BackColor, IntegralHeight = false };
            foreach (var value in values) list.Items.Add(new FacetItem(value), facets.TryGetValue(facet, out var chosen) && chosen.Contains(value));
            list.ItemCheck += (_, e) =>
            {
                if (!facets.TryGetValue(facet, out var chosen)) facets[facet] = chosen = new(StringComparer.OrdinalIgnoreCase);
                string value = ((FacetItem)list.Items[e.Index]).Value;
                if (e.NewValue == CheckState.Checked) chosen.Add(value); else chosen.Remove(value);
                if (IsHandleCreated) BeginInvoke(new Action(RenderProducts));
            };
            sidebar.Controls.Add(list);
        }
        var reset = Shop67Ui.Button(T.L("Puhasta filtrid", "Сбросить фильтры", "Clear filters"), () =>
        { facets.Clear(); query = ""; minimum = 0; maximum = 1000000; inStock = false; selectedOffer = null; Build(); });
        reset.Width = 210; sidebar.Controls.Add(reset);
    }

    private void RenderProducts()
    {
        if (resultsPanel == null || IsDisposed) return;
        resultsPanel.SuspendLayout(); ShopStyle.Clear(resultsPanel);
        if (home)
        {
            heading.Text = T.L("Värsked pakkumised", "Предложения и акции", "Latest offers");
            foreach (var offer in promotions.Where(p => p.Active(DateTimeOffset.UtcNow)).OrderBy(p => p.Position)) AddOffer(offer);
            if (!promotions.Any(p => p.Active(DateTimeOffset.UtcNow))) resultsPanel.Controls.Add(Shop67Ui.Label(T.L("Aktiivseid kampaaniaid praegu pole. Vaata kataloogi.", "Сейчас нет действующих акций. Посмотрите каталог.", "No promotions are active. Explore the catalog."), 12));
            resultsPanel.Controls.Add(Shop67Ui.Label(T.L("Avasta kolm kategooriat", "Выбери свою категорию", "Explore our three categories"), 20, true));
            foreach (string key in CatalogRules.Categories)
            { var button = Shop67Ui.Button(T.Category(key) + "  →", () => Navigate(key, false)); button.Width = 400; resultsPanel.Controls.Add(button); }
            resultsPanel.Controls.Add(Shop67Ui.Label(T.L("Valik kataloogist", "Товары из каталога", "From the catalog"), 20, true));
        }
        var filter = new CatalogFilter { Category = category, Query = query, Minimum = minimum, Maximum = maximum, InStock = inStock, Facets = facets };
        IEnumerable<ShopProduct> found = products.Where(filter.Matches);
        if (selectedOffer != null)
        {
            var offer = promotions.FirstOrDefault(p => p.Id == selectedOffer && p.Active(DateTimeOffset.UtcNow));
            found = offer == null ? Enumerable.Empty<ShopProduct>() : found.Where(offer.Matches);
        }
        found = sortOrder switch { 1 => found.OrderBy(p => p.Price), 2 => found.OrderByDescending(p => p.Price), 3 => found.OrderBy(p => p.DisplayName), _ => found.OrderBy(p => p.Id) };
        var visible = (home ? found.Take(6) : found).ToArray();
        if (!home) heading.Text = T.Category(category) + " · " + visible.Length;
        foreach (var product in visible)
        {
            var card = new ProductListItem(product, () => OpenProduct(product), () => AddToCart(product, 1)); card.Tag = product; card.UpdateAvailability(cart.Quantity(product.Id)); resultsPanel.Controls.Add(card);
        }
        if (visible.Length == 0) resultsPanel.Controls.Add(Shop67Ui.Label(loading ? T.L("Laadimine…", "Загрузка…", "Loading…") : T.L("Sobivaid tooteid ei leitud. Muuda filtreid.", "Подходящих товаров нет. Измените фильтры.", "No matching products. Adjust your filters."), 13));
        ResizeCards(); resultsPanel.ResumeLayout(true); UpdateCart();
    }

    private void AddOffer(Promotion offer)
    {
        var panel = new TableLayoutPanel { Height = 230, ColumnCount = 2, RowCount = 1, Padding = new Padding(22), Margin = new Padding(0, 0, 0, 16), BackColor = Color.FromArgb(255, 235, 195) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        var text = ShopStyle.Stack(); text.AutoScroll = false;
        text.Controls.Add(Shop67Ui.Label(offer.Title.Value + (offer.Percent > 0 ? $"  −{offer.Percent:0.##}%" : ""), 22, true));
        var description = Shop67Ui.Label(offer.Description.Value, 11); description.MaximumSize = new Size(650, 0); text.Controls.Add(description);
        text.Controls.Add(Shop67Ui.Label(T.L("Kehtib kuni ", "Действует до ", "Ends ") + offer.Ends.ToLocalTime().ToString("dd.MM.yyyy HH:mm"), 9));
        var open = Shop67Ui.Button(T.L("Vaata pakkumist →", "Посмотреть предложение →", "Explore offer →"), () =>
        { home = false; category = offer.Category; selectedOffer = offer.Id; facets.Clear(); Render(); }, true);
        open.Width = 245; text.Controls.Add(open); panel.Controls.Add(text, 0, 0);
        if (!string.IsNullOrWhiteSpace(offer.Image)) panel.Controls.Add(ShopImages.Picture(offer.Image), 1, 0);
        else panel.Controls.Add(new Shop67Logo { ForeColor = ShopStyle.Navy, Dock = DockStyle.Fill }, 1, 0);
        resultsPanel.Controls.Add(panel);
    }

    private void ResizeCards()
    {
        if (resultsPanel == null) return;
        foreach (Control control in resultsPanel.Controls)
        { control.Width = Math.Max(600, resultsPanel.ClientSize.Width - 32); if (control is Label label) label.MaximumSize = new Size(control.Width, 0); }
    }
    private bool AddToCart(ShopProduct product, int quantity)
    {
        try { cart.Add(product, quantity); UpdateCart(); return true; }
        catch (InvalidOperationException ex) { MessageBox.Show(this, ex.Message, T.Legacy("Ostukorv")); return false; }
    }
    private void UpdateCart()
    {
        if (basket == null) return;
        basket.Text = T.Legacy("Ostukorv") + $" · {cart.Count} | " + T.Money(cart.Total);
        foreach (var card in resultsPanel.Controls.OfType<ProductListItem>()) if (card.Tag is ShopProduct p) card.UpdateAvailability(cart.Quantity(p.Id));
    }
    private void OpenProduct(ShopProduct product)
    {
        using var detail = new ProductDetailsForm(product, cart, AddToCart);
        if (detail.ShowDialog(this) == DialogResult.OK) OpenCart(); UpdateCart();
    }
    private void OpenCart()
    {
        if (loading) return;
        using var form = new CartForm(cart, store); form.ShowDialog(this); UpdateCart();
        if (form.Order is not { } order) return;
        Reload(); using var confirmation = new OrderConfirmationForm(order); confirmation.ShowDialog(this);
    }
    private sealed record FacetItem(string Value) { public override string ToString() => T.Value(Value); }
}
