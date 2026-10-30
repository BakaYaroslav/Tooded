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
            throw new InvalidOperationException("Täida saatja e-post, server, port ja rakenduse parool. Gmaili jaoks kasuta rakenduse parooli.");
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
    private readonly TextBox host, address, password;
    private readonly NumericUpDown port;
    private readonly CheckBox ssl;
    public MailSettingsForm() : base("E-posti seaded")
    {
        Size = new Size(820, 600);
        var settings = MailSettings.Load();
        Fields.Controls.Add(ShopStyle.Label("Tellimuste kinnitused saadetakse selle postkasti kaudu. Gmail vajab rakenduse parooli.", 11));
        address = TextField("Saatja e-post", settings.SenderEmail);
        host = TextField("SMTP server", settings.Host);
        port = Number("Port", settings.Port, 65535);
        password = TextField("Rakenduse parool", settings.Password); password.UseSystemPasswordChar = true;
        ssl = new CheckBox { Text = "Kasuta turvalist ühendust (TLS)", Checked = settings.EnableSsl, Width = 500 }; Fields.Controls.Add(ssl);
        Fields.Controls.Add(ShopStyle.Label("Parool salvestatakse krüpteeritult selle Windowsi kasutaja jaoks. Seda ei lisata projekti ega GitHubi.", 10));
    }
    protected override void Save() => new MailSettings { Host = host.Text.Trim(), Port = (int)port.Value,
        SenderEmail = address.Text.Trim(), SenderName = "shoping67", Password = password.Text, EnableSsl = ssl.Checked }.Save();
}
