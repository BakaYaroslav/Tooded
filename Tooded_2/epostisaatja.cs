using System.Net;
using System.Net.Mail;
using System.Text;
using Tooded_2.Shop;

namespace Tooded_2;

public static class EpostiSaatja
{
    public static async Task SaadaAsync(string kellele, string teema, string html)
    {
        if (!OrderEmail.ValidAddress(kellele)) throw new InvalidOperationException("Sisesta korrektne e-posti aadress.");
        var settings = MailSettings.Load(); settings.Validate();
        using var message = new MailMessage
        {
            From = new MailAddress(settings.SenderEmail, settings.SenderName), Subject = teema,
            Body = html, IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8
        };
        message.To.Add(kellele.Trim());
        using var smtp = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl, UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(settings.SenderEmail, settings.Password)
        };
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await smtp.SendMailAsync(message, timeout.Token);
    }
    public static bool OnKorrektne(string email) => OrderEmail.ValidAddress(email);
}
