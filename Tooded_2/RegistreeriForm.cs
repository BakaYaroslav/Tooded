using Tooded_2.Shop;
namespace Tooded_2
{
    public partial class RegistreeriForm : Form
    {
        // Uus kasutajanimi, mida LoginForm saab pärast registreerimist lugeda
        public string Kasutajanimi { get; private set; } = "";

        public RegistreeriForm()
        {
            InitializeComponent();
        }

        // T.Legacy("Registreeri"): kontrollib andmeid ja salvestab uue kasutaja (alati rolliga Klient)
        private void RegistreeriBtn_Click(object? sender, EventArgs e)
        {
            string nimi = kasutajaEntry.Text.Trim();

            if (nimi.Length < 3)
            {
                MessageBox.Show(T.Legacy("Kasutajanimi peab olema vähemalt 3 märki!"));
                return;
            }

            if (paroolEntry.Text.Length < 4)
            {
                MessageBox.Show(T.Legacy("Parool peab olema vähemalt 4 märki!"));
                return;
            }

            if (paroolEntry.Text != parool2Entry.Text)
            {
                MessageBox.Show(T.Legacy("Paroolid ei ühti!"));
                return;
            }

            try
            {
                using var db = new AppDbContext();

                if (db.Kasutajatabel.Any(k => k.Kasutajanimi == nimi))
                {
                    MessageBox.Show(T.Legacy("Selline kasutajanimi on juba olemas!"));
                    return;
                }

                db.Kasutajatabel.Add(new Kasutaja
                {
                    Kasutajanimi = nimi,
                    ParoolHash = Parool.Hashi(paroolEntry.Text),
                    Roll = "Klient"   // admini registreerida ei saa
                });
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(T.Legacy("Viga: ") + (ex.InnerException?.Message ?? ex.Message));
                return;
            }

            Kasutajanimi = nimi;
            MessageBox.Show(T.Legacy("Registreerimine õnnestus! Nüüd saad sisse logida."));
            DialogResult = DialogResult.OK;
        }

        // T.Legacy("Tühista"): sulgeb akna
        private void TuhistaBtn_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}