using System.Text.Json.Serialization;

namespace Tooded_2;
internal sealed class MailSettingsForm : EditorForm
{
    private readonly TextBox address, password;
    public MailSettingsForm() : base(() => UiText.Get("EmailSettings"))
    {
        Size = new Size(820, 480);
        MinimumSize = new Size(700, 430);
        var settings = MailSettings.Load();
        var help = ShopUi.Label(() => UiText.Get("MailSettingsHelp"), 11);
        help.MaximumSize = new Size(710, 0);
        Fields.Controls.Add(help);
        address = TextField(() => UiText.Get("SenderGmailAddress"), settings.SenderEmail);
        address.AccessibleName = "mail-sender";
        password = TextField(() => UiText.Get("GmailAppPassword"), settings.Password);
        password.UseSystemPasswordChar = true;
        password.AccessibleName = "mail-password";
        var hint = ShopUi.Label(() => UiText.Get("MailPasswordHelp"), 10);
        hint.MaximumSize = new Size(710, 0);
        Fields.Controls.Add(hint);
    }

    protected override void Save() => new MailSettings
    {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        SenderEmail = address.Text.Trim(),
        SenderName = "shoping67",
        Password = password.Text
    }.Save();
}
