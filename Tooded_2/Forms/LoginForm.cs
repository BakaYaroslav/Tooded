namespace Tooded_2;
public partial class LoginForm : LocalizedForm
{
    // Sisseloginud kasutaja roll, mida Program.cs loeb
    public string Roll { get; private set; } = "";

    public LoginForm()
    {
        InitializeComponent();
        // Отдельная строка сверху оставляет место для любого языка.
        foreach (Control control in Controls)
            control.Top += 40;
        ClientSize = new Size(400, ClientSize.Height + 40);
        kasutajaLabel.Left = paroolLabel.Left = 24;
        kasutajaEntry.Left = paroolEntry.Left = 190;
        loginBtn.Left = 24;
        loginBtn.Width = 165;
        registreeriBtn.Left = 211;
        registreeriBtn.Width = 165;
        RefreshLocalizedValues();
        var comboLanguage = LanguageManager.CreateSelector();
        comboLanguage.Location = new Point(ClientSize.Width - comboLanguage.Width - 12, 10);
        comboLanguage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        Controls.Add(comboLanguage);
    }

    protected override void RefreshLocalizedValues()
    {
        pealkiriLabel.Left = (ClientSize.Width - pealkiriLabel.PreferredSize.Width) / 2;
    }

    // Logi sisse: kontrollib kasutajanime ja parooli
    private void LoginBtn_Click(object? sender, EventArgs e)
    {
        string nimi = kasutajaEntry.Text.Trim();
        if (nimi == "" || paroolEntry.Text == "")
        {
            MessageBox.Show(UiText.Get("EnterYourUsernameAndPassword"));
            return;
        }

        using var db = new AppDbContext();
        var kasutaja = db.Kasutajatabel.FirstOrDefault(k => k.Kasutajanimi == nimi);
        // Sama veateade mõlemal juhul, et ei paljastaks, kas kasutaja on olemas
        if (kasutaja == null || !Parool.Kontrolli(paroolEntry.Text, kasutaja.ParoolHash))
        {
            MessageBox.Show(UiText.Get("IncorrectUsernameOrPassword"));
            paroolEntry.Clear();
            return;
        }

        Roll = kasutaja.Roll;
        DialogResult = DialogResult.OK;
    }

    // Registreeri: avab registreerimise akna
    private void RegistreeriBtn_Click(object? sender, EventArgs e)
    {
        using var reg = new RegisterForm();
        if (reg.ShowDialog(this) == DialogResult.OK)
        {
            // Paneme uue kasutajanime sisselogimise lahtrisse
            kasutajaEntry.Text = reg.Kasutajanimi;
            paroolEntry.Clear();
            paroolEntry.Focus();
        }
    }
}
