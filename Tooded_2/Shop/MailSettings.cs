using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tooded_2.Shop;

public sealed class MailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = "order1print@gmail.com";
    public string SenderName { get; set; } = "shoping67";
    public bool EnableSsl { get; set; } = true;
    public string ProtectedPassword { get; set; } = "";
    [JsonIgnore] public string Password { get; set; } = "";
    private static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "shoping67", "mail-settings.json");
    public static MailSettings Load(string? path = null)
    {
        MailSettings settings;
        try
        {
            settings = JsonSerializer.Deserialize<MailSettings>(File.ReadAllText(path ?? DefaultPath)) ?? new();
            if (settings.ProtectedPassword.Length > 0)
                settings.Password = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(settings.ProtectedPassword), null, DataProtectionScope.CurrentUser));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or CryptographicException or FormatException)
        { settings = new(); }
        if (path == null)
        {
            settings.Host = Environment.GetEnvironmentVariable("SHOPING67_SMTP_HOST") ?? settings.Host;
            if (int.TryParse(Environment.GetEnvironmentVariable("SHOPING67_SMTP_PORT"), out int port)) settings.Port = port;
            settings.SenderEmail = Environment.GetEnvironmentVariable("SHOPING67_SMTP_EMAIL") ?? settings.SenderEmail;
            settings.Password = Environment.GetEnvironmentVariable("SHOPING67_SMTP_PASSWORD")
                ?? Environment.GetEnvironmentVariable("TOODED_SMTP_PAROOL") ?? settings.Password;
        }
        return settings;
    }
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host) || Host.Contains('\r') || Host.Contains('\n') || Port < 1 || Port > 65535
            || !OrderEmail.ValidAddress(SenderEmail) || string.IsNullOrWhiteSpace(Password))
            throw new InvalidOperationException("Kontrolli saatja Gmaili aadressi ja rakenduse parooli. Vajadusel salvesta e-posti seaded uuesti.");
    }
    public void Save(string? path = null)
    {
        Validate();
        ProtectedPassword = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(Password), null, DataProtectionScope.CurrentUser));
        string destination = path ?? DefaultPath;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
        string temporary = destination + "." + Guid.NewGuid().ToString("N");
        try { File.WriteAllText(temporary, JsonSerializer.Serialize(this)); File.Move(temporary, destination, true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}

internal sealed class MailSettingsForm : ShopEditorForm
{
    private readonly TextBox address, password;
    public MailSettingsForm() : base("E-posti seaded")
    {
        Size = new Size(820, 480); MinimumSize = new Size(700, 430);
        var settings = MailSettings.Load();
        var help = ShopStyle.Label("Sisesta Gmaili aadress, millelt pood tellimuste kinnitusi saadab. Ühenduse seadistame automaatselt.", 11);
        help.MaximumSize = new Size(710, 0); Fields.Controls.Add(help);
        address = TextField("Saatja Gmaili aadress", settings.SenderEmail); address.AccessibleName = "mail-sender";
        password = TextField("Gmaili rakenduse parool", settings.Password); password.UseSystemPasswordChar = true; password.AccessibleName = "mail-password";
        var hint = ShopStyle.Label("Kasuta Google'i kontol loodud rakenduse parooli, mitte oma tavalist parooli. Salvestatud parool jääb selle Windowsi kasutaja jaoks krüpteerituks.", 10);
        hint.MaximumSize = new Size(710, 0); Fields.Controls.Add(hint);
    }
    protected override void Save() => new MailSettings
    {
        Host = "smtp.gmail.com", Port = 587, EnableSsl = true,
        SenderEmail = address.Text.Trim(), SenderName = "shoping67", Password = password.Text
    }.Save();
}
