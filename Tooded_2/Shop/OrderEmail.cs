using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace Tooded_2.Shop;

public static class OrderEmail
{
    public static bool ValidAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\r') || value.Contains('\n')) return false;
        try { var mail = new MailAddress(value.Trim()); return mail.Address == value.Trim() && mail.Host.Contains('.'); }
        catch (FormatException) { return false; }
    }
    public static string Subject(DemoOrder order) => $"shoping67 · Tellimus {order.Number} on töös";
    public static string Html(DemoOrder order)
    {
        string Encode(string value) => WebUtility.HtmlEncode(value);
        string rows = string.Join("", order.Lines.Select(line => $"<tr><td>{Encode(line.Product.DisplayName)}</td><td>{line.Quantity}</td><td>{line.Total.ToString("0.00", CultureInfo.GetCultureInfo("et-EE"))} €</td></tr>"));
        return "<!doctype html><html lang=\"et\"><body style=\"font-family:Arial,sans-serif;color:#18222d\">" +
            "<h1>shoping67</h1><h2>Makse õnnestus!</h2><p>Tere!</p>" +
            $"<p>Teie tellimus <strong>{Encode(order.Number)}</strong> on vastu võetud ja töös.</p>" +
            $"<p>Eeldatav tarnekuupäev: <strong>{order.ExpectedDelivery:dd.MM.yyyy}</strong> — umbes kahe nädala pärast.</p>" +
            "<table cellpadding=\"8\"><tr><th>Toode</th><th>Kogus</th><th>Summa</th></tr>" + rows + "</table>" +
            $"<p><strong>Kokku: {order.Total.ToString("0.00", CultureInfo.GetCultureInfo("et-EE"))} €</strong></p>" +
            "<p>Aitäh tellimuse eest!</p><hr><p style=\"color:#5c6975;font-size:12px\">Õppepoe näidistellimus. Makse on simuleeritud, päris raha ei võeta ja kaupa ei saadeta.</p></body></html>";
    }
}

public sealed record CheckoutResult(DemoOrder Order, bool EmailSent, string EmailStatus);

public sealed class CheckoutFlow(IShopStore store, Func<DemoOrder, Task> sendEmail)
{
    private int working;
    public bool InProgress => Volatile.Read(ref working) != 0;
    public CheckoutResult? Result { get; private set; }
    public async Task<CheckoutResult> PayAsync(ShoppingCart cart, string email)
    {
        Enter();
        try
        {
            if (Result != null) throw new InvalidOperationException("Tellimus on juba tasutud.");
            if (!OrderEmail.ValidAddress(email)) throw new InvalidOperationException("Sisesta korrektne e-posti aadress.");
            var lines = cart.Lines.ToArray();
            var order = (await Task.Run(() => store.Checkout(lines))) with { CustomerEmail = email.Trim() };
            Result = new(order, false, "Kinnituskirja saatmine…");
            cart.Clear(); // A mail failure must never repeat checkout or consume stock twice.
            return Result = await Deliver(order);
        }
        finally { Interlocked.Exchange(ref working, 0); }
    }
    public async Task<CheckoutResult> RetryEmailAsync()
    {
        Enter();
        try
        {
            var current = Result ?? throw new InvalidOperationException("Tellimus puudub.");
            return Result = current.EmailSent ? current : await Deliver(current.Order);
        }
        finally { Interlocked.Exchange(ref working, 0); }
    }
    private void Enter()
    {
        if (Interlocked.CompareExchange(ref working, 1, 0) != 0)
            throw new InvalidOperationException("Tellimust juba töödeldakse. Palun oota.");
    }
    private async Task<CheckoutResult> Deliver(DemoOrder order)
    {
        try { await sendEmail(order); return new(order, true, "Tellimuse kinnitus on saadetud aadressile " + order.CustomerEmail + "."); }
        catch (Exception) { return new(order, false, "Makse õnnestus, kuid kinnituskirja ei saanud saata. Saad saatmist uuesti proovida."); }
    }
}
