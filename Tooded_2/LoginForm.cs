using Tooded_2.Shop;
namespace Tooded_2
{
    public partial class LoginForm : LocalizedForm
    {
        // Sisseloginud kasutaja roll, mida Program.cs loeb
        public string Roll { get; private set; } = "";

        public LoginForm()
        {
            InitializeComponent();
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + 42);
            var languageLabel = new Label { Name = "languageLabel", AutoSize = true, Location = new Point(30, 215) };
            LanguageManager.Bind(languageLabel, "Text", () => UiText.Get("Language"));
            var comboLanguage = LanguageManager.CreateSelector(); comboLanguage.Location = new Point(140, 211);
            Controls.Add(languageLabel); Controls.Add(comboLanguage);


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
            using var reg = new RegistreeriForm();
            if (reg.ShowDialog(this) == DialogResult.OK)
            {
                // Paneme uue kasutajanime sisselogimise lahtrisse
                kasutajaEntry.Text = reg.Kasutajanimi;
                paroolEntry.Clear();
                paroolEntry.Focus();
            }
        }
    }
}