using System.Globalization;
using System.Text;

namespace Tooded_2;

/// <summary>Результат оформленной покупки. Содержит снимок товаров и оплаченных цен.</summary>
public sealed record ShopOrder(string Number, DateTimeOffset Created, IReadOnlyList<CartLine> Lines)
{
    public string CustomerEmail { get; init; } = "";
    public string CultureName { get; init; } = CultureInfo.CurrentUICulture.Name;
    public DateOnly ExpectedDelivery => DateOnly.FromDateTime(Created.LocalDateTime).AddDays(14);
    public decimal Total => Lines.Sum(line => line.Total);

    public string Receipt
    {
        get
        {
            var receipt = new StringBuilder();
            receipt.AppendLine(UiText.Get("Shoping67DEMOORDER"));
            receipt.AppendLine($"{UiText.Get("Order")}: {Number}");
            receipt.AppendLine($"{UiText.Get("Date")}: {Created:dd.MM.yyyy HH:mm}");
            receipt.AppendLine(UiText.Get("DEMOPAYMENTNoMoneyWasCharged"));
            receipt.AppendLine();
            receipt.AppendLine(UiText.Get("YourOrderIsBeingProcessed"));
            receipt.AppendLine(UiText.Format("EstimatedDeliveryLine", ExpectedDelivery));
            if (CustomerEmail.Length > 0) receipt.AppendLine(UiText.Get("Email") + CustomerEmail);
            foreach (var line in Lines)
                receipt.AppendLine($"{line.Product.DisplayName} | {line.Quantity} × {line.Product.Price:0.00} € = {line.Total:0.00} €");
            receipt.AppendLine();
            receipt.AppendLine($"{UiText.Get("TOTAL")}: {Total:0.00} €");
            receipt.AppendLine(UiText.Get("ThisIsADemoOrderNoGoodsAreShipped"));
            return receipt.ToString();
        }
    }

    public static ShopOrder Create(IReadOnlyList<CartLine> lines)
    {
        string number = $"DEMO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        return new ShopOrder(number, DateTimeOffset.Now, lines.ToArray());
    }
}
