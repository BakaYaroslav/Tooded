namespace Tooded_2;
public sealed record CheckoutResult(ShopOrder Order, bool EmailSent, string EmailStatusKey)
{
    public string EmailStatus => UiText.Get(EmailStatusKey) + (EmailSent ? Order.CustomerEmail + "." : "");
}
