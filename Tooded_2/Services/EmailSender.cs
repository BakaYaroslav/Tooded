using System.Net;
using System.Net.Mail;

namespace Tooded_2;
public static class EmailSender
{
    public static async Task SendOrderConfirmationAsync(ShopOrder order)
    {
        var settings = MailSettings.Load();
        settings.Validate();
        using var message = OrderMailMessage.Create(order, settings);
        await SendAsync(message, settings);
    }

    private static async Task SendAsync(MailMessage message, MailSettings settings)
    {
        using var smtp = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(settings.SenderEmail, settings.Password)
        };
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await smtp.SendMailAsync(message, timeout.Token);
    }
}
