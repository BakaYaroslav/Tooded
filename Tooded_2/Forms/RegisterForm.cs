namespace Tooded_2;
public partial class RegisterForm : LocalizedForm
{
    // Uus kasutajanimi, mida LoginForm saab pärast registreerimist lugeda
    public string Kasutajanimi { get; private set; } = "";

    public RegisterForm()
    {
        InitializeComponent();
        ClientSize = new Size(400, ClientSize.Height);
        kasutajaLabel.Left = paroolLabel.Left = parool2Label.Left = 24;
        kasutajaEntry.Left = paroolEntry.Left = parool2Entry.Left = 195;
        registreeriBtn.Left = 24;
        registreeriBtn.Width = 170;
        tuhistaBtn.Left = 210;
        tuhistaBtn.Width = 166;
    }

    // Registreeri: kontrollib andmeid ja salvestab uue kasutaja (alati rolliga Klient)
    private void RegistreeriBtn_Click(object? sender, EventArgs e)
    {
        string nimi = kasutajaEntry.Text.Trim();
        if (nimi.Length < 3)
        {
            MessageBox.Show(UiText.Get("UsernameTooShort"));
            return;
        }

        if (paroolEntry.Text.Length < 4)
        {
            MessageBox.Show(UiText.Get("PasswordTooShort"));
            return;
        }

        if (paroolEntry.Text != parool2Entry.Text)
        {
            MessageBox.Show(UiText.Get("ThePasswordsDoNotMatch"));
            return;
        }

        try
        {
            using var db = new AppDbContext();
            if (db.Kasutajatabel.Any(k => k.Kasutajanimi == nimi))
            {
                MessageBox.Show(UiText.Get("ThisUsernameAlreadyExists"));
                return;
            }

            db.Kasutajatabel.Add(new Kasutaja { Kasutajanimi = nimi, ParoolHash = Parool.Hashi(paroolEntry.Text), Roll = "Klient" // admini registreerida ei saa
 });
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            MessageBox.Show(UiText.Get("Error") + (ex.InnerException?.Message ?? ex.Message));
            return;
        }

        Kasutajanimi = nimi;
        MessageBox.Show(UiText.Get("RegistrationSucceeded"));
        DialogResult = DialogResult.OK;
    }

    // Tühista: sulgeb akna
    private void TuhistaBtn_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
    }
}
