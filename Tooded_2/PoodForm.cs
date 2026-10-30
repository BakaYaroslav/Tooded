using Tooded_2.Shop;

namespace Tooded_2;

public partial class PoodForm : Form
{
    private readonly IShopStore store;
    private readonly ShoppingCart cart = new();
    private IReadOnlyList<ShopProduct> products = Array.Empty<ShopProduct>();
    private bool loading;

    public PoodForm() : this(new DatabaseShopStore()) { }

    public PoodForm(IShopStore store)
    {
        this.store = store; InitializeComponent();
        search.TextChanged += (_, _) => Render(); categories.SelectedIndexChanged += (_, _) => Render();
        sort.SelectedIndexChanged += (_, _) => Render(); inStock.CheckedChanged += (_, _) => Render();
        productsPanel.SizeChanged += (_, _) => ResizeCards(); basket.Click += (_, _) => OpenCart();
        Shown += (_, _) => Reload();
    }

    private async void Reload()
    {
        if (loading || IsDisposed) return;
        loading = true;
        try
        {
            UseWaitCursor = true;
            status.Text = "Toodete laadimine… Esimesel korral laaditakse veebikataloog ja pildid.";
            var loaded = await Task.Run(() => store.GetProducts());
            if (IsDisposed) return;
            var selected = categories.SelectedItem as string;
            loading = true; products = loaded; categories.Items.Clear(); categories.Items.Add("Kõik tooted");
            foreach (string category in products.Select(p => p.Category).Distinct().OrderBy(c => c)) categories.Items.Add(category);
            categories.SelectedItem = selected;
            if (categories.SelectedIndex < 0) categories.SelectedIndex = 0;
            status.Text = store.CatalogStatus;
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            status.Text = "Kataloogi ei saanud uuendada. Ostukorv on alles.";
            MessageBox.Show(this, "Tooteid ei saanud laadida. Kontrolli internetti ja LocalDB ühendust ning vajuta „Uuenda”. Ilma andmebaasita kasuta --demo.\n\n" + ex.Message,
                "Kataloog pole saadaval", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            loading = false;
            if (!IsDisposed) { UseWaitCursor = false; Render(); }
        }
    }

    private void Render()
    {
        if (loading) return;
        productsPanel.SuspendLayout(); ShopStyle.Clear(productsPanel);
        string query = search.Text.Trim(); string? category = categories.SelectedIndex > 0 ? categories.SelectedItem as string : null;
        IEnumerable<ShopProduct> filtered = products.Where(p => (category == null || p.Category == category)
            && (!inStock.Checked || p.Stock > 0)
            && (p.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) || p.Category.Contains(query, StringComparison.CurrentCultureIgnoreCase)));
        filtered = sort.SelectedIndex switch
        {
            1 => filtered.OrderBy(p => p.Price), 2 => filtered.OrderByDescending(p => p.Price),
            3 => filtered.OrderBy(p => p.Name), _ => filtered.OrderBy(p => p.Id)
        };
        var visible = filtered.ToArray(); results.Text = $"Tooted · {visible.Length}";
        foreach (var product in visible)
        {
            var card = new ProductListItem(product, () => OpenProduct(product), () => AddToCart(product, 1));
            card.Tag = product; card.UpdateAvailability(cart.Quantity(product.Id)); productsPanel.Controls.Add(card);
        }
        if (visible.Length == 0)
        {
            var empty = ShopStyle.Label(products.Count == 0 ? "Kataloogis pole veel tooteid." : "Sobivaid tooteid ei leitud. Muuda otsingut või filtreid.", 13);
            empty.AutoSize = false; empty.Height = 90; productsPanel.Controls.Add(empty);
        }
        ResizeCards(); productsPanel.ResumeLayout(); UpdateCart();
    }

    private void ResizeCards()
    {
        foreach (Control card in productsPanel.Controls) card.Width = Math.Max(650, productsPanel.ClientSize.Width - 32);
    }

    private bool AddToCart(ShopProduct product, int quantity)
    {
        try { cart.Add(product, quantity); status.Text = $"Lisatud: {product.Name} · {quantity} tk"; UpdateCart(); return true; }
        catch (InvalidOperationException ex) { MessageBox.Show(this, ex.Message, "Ostukorv"); return false; }
    }

    private void UpdateCart()
    {
        basket.Text = $"Ostukorv · {cart.Count}  |  {cart.Total:0.00} €";
        foreach (var card in productsPanel.Controls.OfType<ProductListItem>())
            if (card.Tag is ShopProduct product) card.UpdateAvailability(cart.Quantity(product.Id));
    }

    private void OpenProduct(ShopProduct product)
    {
        using var detail = new ProductDetailsForm(product, cart, AddToCart);
        if (detail.ShowDialog(this) == DialogResult.OK) OpenCart();
        UpdateCart();
    }

    private void OpenCart()
    {
        if (loading) return;
        using var basketForm = new CartForm(cart, store); basketForm.ShowDialog(this); UpdateCart();
        if (basketForm.Order is not { } order) return;
        Reload();
        using var confirmation = new OrderConfirmationForm(order); confirmation.ShowDialog(this);
    }
}
