using System.Globalization;
using System.Net.Mail;
using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace Tooded_2.Shop;

public static class OrderPdf
{
    private static readonly object gate = new();
    private static bool fontsReady;
    public static string FileName(DemoOrder order)
    {
        string number = new(order.Number.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').Take(90).ToArray());
        return "shoping67-" + (number.Length == 0 ? "ostutsekk" : number) + ".pdf";
    }
    public static byte[] Create(DemoOrder order)
    {
        lock (gate)
        {
            if (!fontsReady) { GlobalFontSettings.FontResolver = new ReceiptFonts(); fontsReady = true; }
            using var document = new PdfDocument();
            document.Info.Title = "shoping67 · Ostutšekk " + order.Number;
            document.Info.Author = "shoping67";
            document.Info.Subject = "Makse õnnestus · Näidismakse kinnitus";
            var options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            var normal = new XFont("shoping67-receipt", 10, XFontStyle.Regular, options);
            var small = new XFont("shoping67-receipt", 8, XFontStyle.Regular, options);
            var bold = new XFont("shoping67-receipt", 10, XFontStyle.Bold, options);
            var title = new XFont("shoping67-receipt", 25, XFontStyle.Bold, options);
            var totalFont = new XFont("shoping67-receipt", 16, XFontStyle.Bold, options);
            var navy = new XSolidBrush(XColor.FromArgb(19, 31, 46));
            var green = new XSolidBrush(XColor.FromArgb(0, 117, 76));
            var muted = new XSolidBrush(XColor.FromArgb(92, 105, 117));
            PdfPage? page = null; XGraphics? graphics = null; double y = 0;
            const double margin = 40;
            string Money(decimal value) => value.ToString("0.00", CultureInfo.GetCultureInfo("et-EE")) + " €";
            void Text(string value, XFont font, XBrush brush, double x, double top, double width, bool right = false) =>
                graphics!.DrawString(value, font, brush, new XRect(x, top, width, 20), right ? XStringFormats.TopRight : XStringFormats.TopLeft);
            void NewPage()
            {
                graphics?.Dispose(); page = document.AddPage(); page.Size = PdfSharpCore.PageSize.A4;
                graphics = XGraphics.FromPdfPage(page);
                Text("shoping67", title, navy, margin, 32, 400);
                graphics.DrawLine(new XPen(XColor.FromArgb(255, 166, 35), 3), margin, 67, 174, 67);
                Text("OSTUTŠEKK / MAKSEKINNITUS", bold, navy, margin, 83, 515);
                Text("MAKSTUD — näidismakse õnnestus", bold, green, margin, 105, 515);
                Text("Tellimuse nr: " + order.Number, normal, navy, margin, 130, 515);
                Text("Kuupäev: " + order.Created.LocalDateTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("et-EE")), normal, navy, margin, 147, 515);
                y = 164;
                if (order.CustomerEmail.Length > 0)
                    foreach (string line in Wrap("Maksja e-post: " + order.CustomerEmail, normal, 515, graphics))
                    { Text(line, normal, navy, margin, y, 515); y += 15; }
                Text("Tellimus on vastu võetud ja töös.", normal, navy, margin, y + 2, 515); y += 20;
                Text($"Eeldatav tarne: {order.ExpectedDelivery:dd.MM.yyyy} (umbes kahe nädala pärast)", normal, navy, margin, y, 515); y += 32;
                graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(244, 246, 248)), margin, y - 4, 515, 25);
                Text("Toode", bold, navy, margin + 6, y, 265);
                Text("Kogus", bold, navy, 309, y, 44, true);
                Text("Hind", bold, navy, 363, y, 82, true);
                Text("Summa", bold, navy, 455, y, 94, true); y += 28;
            }
            void Space(double needed) { if (page == null || y + needed > page.Height.Point - 90) NewPage(); }
            try
            {
                NewPage();
                foreach (var line in order.Lines)
                {
                    Space(32); y += 6; bool first = true;
                    foreach (string part in Wrap(line.Product.DisplayName, normal, 261, graphics!).ToArray())
                    {
                        Space(17); Text(part, normal, navy, margin + 6, y, 261);
                        if (first)
                        {
                            Text(line.Quantity.ToString(CultureInfo.InvariantCulture), normal, navy, 309, y, 44, true);
                            Text(Money(line.Product.Price), normal, navy, 363, y, 82, true);
                            Text(Money(line.Total), normal, navy, 455, y, 94, true); first = false;
                        }
                        y += 15;
                    }
                    y += 6; graphics!.DrawLine(XPens.LightGray, margin, y, 555, y); y += 4;
                }
                Space(100); y += 10;
                Text("KOKKU TASUTUD", totalFont, navy, margin + 6, y, 290);
                Text(Money(order.Total), totalFont, green, 350, y, 199, true); y += 30;
                Text("Makseviis: näidismakse", normal, navy, margin + 6, y, 515); y += 21;
                Text("Aitäh ostu eest!", bold, navy, margin + 6, y, 515);
                graphics!.Dispose(); graphics = null;
                for (int i = 0; i < document.PageCount; i++)
                {
                    page = document.Pages[i]; graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                    double footer = page.Height.Point - 60;
                    graphics.DrawLine(XPens.LightGray, margin, footer - 8, 555, footer - 8);
                    Text("Õppepoe näidistellimus. Päris raha ei võeta ja kaupa ei saadeta.", small, muted, margin, footer, 515);
                    Text("shoping67 · " + order.Number, small, muted, margin, footer + 16, 400);
                    Text($"{i + 1} / {document.PageCount}", small, muted, 460, footer + 16, 95, true);
                    graphics.Dispose(); graphics = null;
                }
                using var output = new MemoryStream(); document.Save(output, false); return output.ToArray();
            }
            finally { graphics?.Dispose(); }
        }
    }
    private static IEnumerable<string> Wrap(string text, XFont font, double width, XGraphics graphics)
    {
        string current = "";
        foreach (string word in text.Replace('\r', ' ').Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (graphics.MeasureString(current.Length == 0 ? word : current + " " + word, font).Width <= width)
            { current = current.Length == 0 ? word : current + " " + word; continue; }
            if (current.Length > 0) { yield return current; current = ""; }
            foreach (var rune in word.EnumerateRunes())
            {
                string next = rune.ToString();
                if (current.Length > 0 && graphics.MeasureString(current + next, font).Width > width)
                { yield return current; current = ""; }
                current += next;
            }
        }
        if (current.Length > 0) yield return current;
        else if (text.Length == 0) yield return "—";
    }
    private sealed class ReceiptFonts : IFontResolver
    {
        public string DefaultFontName => "shoping67-receipt";
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) => new(isBold ? "receipt-bold" : "receipt-regular");
        public byte[] GetFont(string faceName) => File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), faceName == "receipt-bold" ? "arialbd.ttf" : "arial.ttf"));
    }
}

public static class OrderMailMessage
{
    public static MailMessage Create(DemoOrder order, MailSettings settings)
    {
        if (!OrderEmail.ValidAddress(order.CustomerEmail)) throw new InvalidOperationException("Sisesta korrektne e-posti aadress.");
        byte[] pdf = OrderPdf.Create(order);
        var message = new MailMessage
        {
            From = new MailAddress(settings.SenderEmail, settings.SenderName),
            Subject = OrderEmail.Subject(order), Body = OrderEmail.Html(order), IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8
        };
        try
        {
            message.To.Add(order.CustomerEmail);
            var attachment = new Attachment(new MemoryStream(pdf, writable: false), OrderPdf.FileName(order), "application/pdf")
            { NameEncoding = Encoding.UTF8 };
            message.Attachments.Add(attachment);
            return message;
        }
        catch { message.Dispose(); throw; }
    }
}
