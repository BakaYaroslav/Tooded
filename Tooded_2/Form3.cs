using Tooded_2.Shop;
using System.ComponentModel;

namespace Tooded_2
{
    public partial class Form3 : LocalizedForm
    {
        // Funktsioon, mis otsib tooteid andmebaasist (antakse Form2-st)
        private readonly Func<string, List<PoodToode>> otsiTooted;

        private readonly BindingList<OstuRida> korv = new();
        private readonly Dictionary<int, ToodeKaart> kaardid = new();

        /// <summary>Mida osteti (loe pärast DialogResult.OK).</summary>
        public List<OstuRida> Ostud => korv.ToList();

        /// <summary>Lõppsumma.</summary>
        public decimal Kokku => korv.Sum(r => r.Summa);

        /// <summary>Ostja e-post, kuhu tšekk saadetakse.</summary>
        public string Email => epostEntry.Text.Trim();

        public Form3(Func<string, List<PoodToode>> otsiTooted)
        {
            InitializeComponent();
            this.otsiTooted = otsiTooted;
            korvGrid.DataSource = korv;
            UuendaKokku();
        }

        // ---------- Toodete laadimine ----------

        private void Form3_Load(object? sender, EventArgs e) => LaeTooted("");

        private void LaeTooted(string otsing)
        {
            List<PoodToode> tooted;
            try
            {
                Cursor = Cursors.WaitCursor;
                tooted = otsiTooted(otsing);
            }
            catch (Exception ex)
            {
                MessageBox.Show(UiText.Get("UnableToLoadProducts") + ex.Message, UiText.Get("Shop"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            tootedPanel.SuspendLayout();

            // Vanad kaardid ära (Dispose vabastab ka pildid)
            foreach (Control c in tootedPanel.Controls.Cast<Control>().ToList())
                c.Dispose();
            tootedPanel.Controls.Clear();
            kaardid.Clear();

            foreach (var toode in tooted)
            {
                var kaart = new ToodeKaart(toode);
                kaart.SeaSaadaval(Saadaval(toode));
                kaart.LisaKorvi += Kaart_LisaKorvi;
                tootedPanel.Controls.Add(kaart);
                kaardid[toode.Id] = kaart;
            }

            tootedPanel.ResumeLayout();

            LanguageManager.Bind(leitudLabel, "Text", () => tooted.Count == 0
                ? UiText.Get("NoProductsFound")
                : UiText.Format("Found0Products", tooted.Count));
        }

        /// <summary>Laos olev kogus miinus see, mis on juba korvis.</summary>
        private int Saadaval(PoodToode toode) =>
            toode.Kogus - korv.Where(r => r.ToodeId == toode.Id).Sum(r => r.Kogus);

        // ---------- Otsing (ootab 300 ms pärast viimast klahvivajutust) ----------

        private void otsiEntry_TextChanged(object? sender, EventArgs e)
        {
            otsiTimer.Stop();
            otsiTimer.Start();
        }

        private void otsiTimer_Tick(object? sender, EventArgs e)
        {
            otsiTimer.Stop();
            LaeTooted(otsiEntry.Text.Trim());
        }

        // ---------- Ostukorv ----------

        private void Kaart_LisaKorvi(object? sender, int kogus)
        {
            if (sender is not ToodeKaart kaart || kogus <= 0) return;

            var toode = kaart.Toode;
            kogus = Math.Min(kogus, Saadaval(toode));
            if (kogus <= 0) return;

            var olemas = korv.FirstOrDefault(r => r.ToodeId == toode.Id);
            if (olemas != null)
                olemas.Kogus += kogus;
            else
                korv.Add(new OstuRida
                {
                    ToodeId = toode.Id,
                    Nimetus = toode.Nimetus,
                    Kogus = kogus,
                    Hind = toode.Hind
                });

            korv.ResetBindings();
            UuendaKokku();
            kaart.SeaSaadaval(Saadaval(toode));
        }

        private void eemaldaBtn_Click(object? sender, EventArgs e)
        {
            if (korvGrid.CurrentRow?.DataBoundItem is not OstuRida rida) return;

            korv.Remove(rida);
            UuendaKokku();

            if (kaardid.TryGetValue(rida.ToodeId, out var kaart))
                kaart.SeaSaadaval(Saadaval(kaart.Toode));
        }

        private void UuendaKokku()
        {
            LanguageManager.Bind(kokkuLabel, "Text", () => UiText.Format("Total0000", Kokku));
            maksaBtn.Enabled = korv.Count > 0;
            eemaldaBtn.Enabled = korv.Count > 0;
        }

        private void korvGrid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (var nimi in new[] { "Hind", "Summa" })
                if (korvGrid.Columns.Contains(nimi))
                    korvGrid.Columns[nimi]!.DefaultCellStyle.Format = "0.00";
        }

        private void maksaBtn_Click(object? sender, EventArgs e)
        {
            if (korv.Count == 0)
            {
                MessageBox.Show(UiText.Get("YourBasketIsEmpty"), UiText.Get("Shop"));
                return;
            }

            if (!EpostiSaatja.OnKorrektne(Email))
            {
                MessageBox.Show(UiText.Get("EnterAValidEmailAddressForTheReceipt"), UiText.Get("Shop"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                epostEntry.Focus();
                return;
            }

            var vastus = MessageBox.Show(
                UiText.Format("Pay0000TheReceiptWillBeSentTo1", Kokku, Email), UiText.Get("Payment"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (vastus == DialogResult.Yes)
                DialogResult = DialogResult.OK; // sulgeb akna, Form2 loeb Ostud ja Kokku
        }
    }

    public class OstuRida
    {
        [Browsable(false)]
        public int ToodeId { get; set; }

        [DisplayName("Toode")]
        public string Nimetus { get; set; } = "";

        public int Kogus { get; set; }

        public decimal Hind { get; set; }

        public decimal Summa => Kogus * Hind;
    }
}