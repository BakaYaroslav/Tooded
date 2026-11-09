using System.Net.Mail;
using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace Tooded_2;
public static class OrderMailMessage
{
    public static MailMessage Create(ShopOrder order, MailSettings settings)
    {
        if (!OrderEmail.ValidAddress(order.CustomerEmail))
            throw new InvalidOperationException(UiText.Get("EnterAValidEmailAddress"));
        byte[] pdf = OrderPdf.Create(order);
        var message = new MailMessage
        {
            From = new MailAddress(settings.SenderEmail, settings.SenderName),
            Subject = OrderEmail.Subject(order),
            Body = OrderEmail.Html(order),
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        try
        {
            message.To.Add(order.CustomerEmail);
            var attachment = new Attachment(new MemoryStream(pdf, writable: false), OrderPdf.FileName(order), "application/pdf")
            {
                NameEncoding = Encoding.UTF8
            };
            message.Attachments.Add(attachment);
            return message;
        }
        catch
        {
            message.Dispose();
            throw;
        }
    }
}
