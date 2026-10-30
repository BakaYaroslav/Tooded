using Tooded_2.Shop;
namespace Tooded_2
{
    public partial class LoginForm : Form
    {
        // Sisseloginud kasutaja roll, mida Program.cs loeb
        public string Roll { get; private set; } = "";

        public LoginForm()
        {
            InitializeComponent();
            foreach (Control control in Controls) control.Top += 48;
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + 48);
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 4, 0, 0), BackColor = Color.White };
            toolbar.Controls.Add(Shop67Ui.LanguagePicker(() => Shop67Ui.Translate(this)));
            Controls.Add(toolbar);
            Shop67Ui.Translate(this);

        }

        // T.Legacy("Logi sisse"): kontrollib kasutajanime ja parooli
        private void LoginBtn_Click(object? sender, EventArgs e)
        {
            string nimi = kasutajaEntry.Text.Trim();

            if (nimi == "" || paroolEntry.Text == "")
            {
                MessageBox.Show(T.Legacy("Sisesta kasutajanimi ja parool!"));
                return;
            }

            using var db = new AppDbContext();
            var kasutaja = db.Kasutajatabel.FirstOrDefault(k => k.Kasutajanimi == nimi);

            // Sama veateade mõlemal juhul, et ei paljastaks, kas kasutaja on olemas
            if (kasutaja == null || !Parool.Kontrolli(paroolEntry.Text, kasutaja.ParoolHash))
            {
                MessageBox.Show(T.Legacy("Vale kasutajanimi või parool!"));
                paroolEntry.Clear();
                return;
            }

            Roll = kasutaja.Roll;
            DialogResult = DialogResult.OK;
        }

        // T.Legacy("Registreeri"): avab registreerimise akna
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