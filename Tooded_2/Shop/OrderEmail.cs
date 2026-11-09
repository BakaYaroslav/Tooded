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
    public static string Subject(DemoOrder order)
    {
        using var culture = new DocumentCulture(order.CultureName);
        return UiText.Format("Shoping67Order0IsBeingProcessed", order.Number);
    }
    public static string Html(DemoOrder order)
    {
        using var culture = new DocumentCulture(order.CultureName);
        string Encode(string value) => WebUtility.HtmlEncode(value);
        string rows = string.Join("", order.Lines.Select(line => $"<tr><td>{Encode(line.Product.DisplayName)}</td><td>{line.Quantity}</td><td>{Encode(UiText.Money(line.Total))}</td></tr>"));
        return UiText.Format("OrderEmailHtml", Encode(order.Number),
            order.ExpectedDelivery.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture), rows, Encode(UiText.Money(order.Total)));
    }
}

public sealed record CheckoutResult(DemoOrder Order, bool EmailSent, string EmailStatusKey)
{
    public string EmailStatus => UiText.Get(EmailStatusKey) + (EmailSent ? Order.CustomerEmail + "." : "");
}

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
            if (Result != null) throw new InvalidOperationException(UiText.Get("ThisOrderHasAlreadyBeenPaid"));
            if (!OrderEmail.ValidAddress(email)) throw new InvalidOperationException(UiText.Get("EnterAValidEmailAddress"));
            var lines = cart.Lines.ToArray();
            var order = (await Task.Run(() => store.Checkout(lines))) with { CustomerEmail = email.Trim() };
            Result = new(order, false, "SendingConfirmation");
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
            var current = Result ?? throw new InvalidOperationException(UiText.Get("NoOrderIsAvailable"));
            return Result = current.EmailSent ? current : await Deliver(current.Order);
        }
        finally { Interlocked.Exchange(ref working, 0); }
    }
    private void Enter()
    {
        if (Interlocked.CompareExchange(ref working, 1, 0) != 0)
            throw new InvalidOperationException(UiText.Get("YourOrderIsAlreadyBeingProcessedPleaseWait"));
    }
    private async Task<CheckoutResult> Deliver(DemoOrder order)
    {
        try { await sendEmail(order); return new(order, true, "TheOrderConfirmationWasSentTo"); }
        catch (Exception) { return new(order, false, "PaymentSucceededButTheConfirmationEmailCouldNotBeSentYouCanRetry"); }
    }
}
