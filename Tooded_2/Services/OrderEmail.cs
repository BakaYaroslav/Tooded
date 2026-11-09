using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace Tooded_2;
public static class OrderEmail
{
    public static bool ValidAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\r') || value.Contains('\n'))
            return false;
        try
        {
            var mail = new MailAddress(value.Trim());
            return mail.Address == value.Trim() && mail.Host.Contains('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string Subject(ShopOrder order)
    {
        using var culture = new DocumentCulture(order.CultureName);
        return UiText.Format("Shoping67Order0IsBeingProcessed", order.Number);
    }

    public static string Html(ShopOrder order)
    {
        using var culture = new DocumentCulture(order.CultureName);
        string Encode(string value) => WebUtility.HtmlEncode(value);
        string rows = string.Join("", order.Lines.Select(line => $"<tr><td>{Encode(line.Product.DisplayName)}</td><td>{line.Quantity}</td><td>{Encode(UiText.Money(line.Total))}</td></tr>"));
        return UiText.Format("OrderEmailHtml", Encode(order.Number), order.ExpectedDelivery.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture), rows, Encode(UiText.Money(order.Total)));
    }
}
