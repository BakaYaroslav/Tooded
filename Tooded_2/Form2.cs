using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
=======
using System.Globalization;
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)

namespace Tooded_2
{
    public partial class Form2 : Form
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly string _imagesDir = Path.Combine(AppContext.BaseDirectory, "Images");

        private string? valitudPilt;   // полный путь к выбранной картинке
        private int? _valitudId;

        // Üks koht, kus pildid asuvad (projekti kaust, mitte bin\Debug)
        private static readonly string PildiKaust = Path.GetFullPath(@"..\..\..\Images");

        public Form2()
        {
            InitializeComponent();
<<<<<<< HEAD
            _context = new AppDbContext();

            Directory.CreateDirectory(PildiKaust);

            kategooriaBox.DropDownStyle = ComboBoxStyle.DropDownList;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

=======

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.CellClick += dataGridViewTooted_CellClick;
            poodBtn.Click += PoodBtn_Click;

            toodePilt.SizeMode = PictureBoxSizeMode.Zoom;

            lisaKategooriaBtn.Click += LisaKategooriaBtn_Click;
            kustutaKategooriaBtn.Click += KustutaKategooriaBtn_Click;
            puhastaBtn.Click += PuhastaBtn_Click;
            otsiFailBtn.Click += ValiPiltbtn_Click;

            Directory.CreateDirectory(_imagesDir);
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)
            LaeKategooriad();
            LaeTooted();
        }

<<<<<<< HEAD
        // ---------- Andmete laadimine ----------

        private void LaeKategooriad()
        {
            // Enne DisplayMember/ValueMember, siis DataSource
            kategooriaBox.DisplayMember = "nimetus";
            kategooriaBox.ValueMember = "Id";
            kategooriaBox.DataSource = _context.Kategooriatabel.ToList();
=======
        // ---------- загрузка данных ----------

        private void LaeKategooriad()
        {
            var kategooriad = _db.Kategooriatabel.AsNoTracking().ToList();

            kategooriaBox.DataSource = null;
            kategooriaBox.DisplayMember = "nimetus";
            kategooriaBox.ValueMember = "Id";
            kategooriaBox.DataSource = kategooriad;
            kategooriaBox.SelectedIndex = kategooriad.Count > 0 ? 0 : -1;
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)
        }

        private void LaeTooted()
        {
<<<<<<< HEAD
            dataGridView1.DataSource = _context.Toodetabel
                .Include(t => t.Kategooria)
=======
            dataGridView1.DataSource = _db.Toodetabel
                .AsNoTracking()
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)
                .Select(t => new
                {
                    t.Id,
                    t.Toodenimetus,
                    t.Kogus,
                    t.Hind,
                    t.KategooriaId,
<<<<<<< HEAD
                    Kategooria = t.Kategooria.nimetus,
=======
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)
                    t.Pilt
                })
                .ToList();

<<<<<<< HEAD
            if (dataGridView1.Columns["KategooriaId"] != null)
                dataGridView1.Columns["KategooriaId"].Visible = false;
        }

        // ---------- Abimeetodid ----------

        // Laeb pildi nii, et fail ei jää lukku
        private static Image LaePiltLukustamata(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var tmp = Image.FromStream(fs);
            return new Bitmap(tmp);
        }

        private void NaitaPilti(string path)
        {
            toodePilt.Image?.Dispose();
            toodePilt.Image = null;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                toodePilt.Image = LaePiltLukustamata(path);
        }

        // Kopeerib valitud pildi Images kausta ja tagastab failinime (või null)
        private string KopeeriPilt()
        {
            if (string.IsNullOrEmpty(valitudPilt))
                return null;

            string failinimi = Path.GetFileName(valitudPilt);
            string siht = Path.Combine(PildiKaust, failinimi);

            // Kui fail on juba Images kaustas, ära kopeeri iseendale
            if (!string.Equals(Path.GetFullPath(valitudPilt), siht, StringComparison.OrdinalIgnoreCase))
                File.Copy(valitudPilt, siht, true);

            return failinimi;
        }

        private bool LoeVorm(string nimi, string kogusTekst, string hindTekst,
                             out int kogus, out float hind, out int katId)
        {
            kogus = 0; hind = 0; katId = 0;

            if (string.IsNullOrWhiteSpace(nimi))
            {
                MessageBox.Show("Sisesta toote nimetus!");
                return false;
            }
            if (!int.TryParse(kogusTekst, out kogus))
            {
                MessageBox.Show("Kogus peab olema täisarv!");
                return false;
            }
            if (!float.TryParse(hindTekst, out hind))
            {
                MessageBox.Show("Hind on vigane!");
                return false;
            }
            if (kategooriaBox.SelectedValue is not int id)
            {
                MessageBox.Show("Vali kategooria!");
                return false;
            }

            katId = id;
            return true;
        }

        private int? ValitudId()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vali tabelist toode!");
                return null;
            }
            return (int)dataGridView1.SelectedRows[0].Cells["Id"].Value;
        }

        // ---------- Nupud ----------

        private void btnValiPilt_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Pildid|*.jpg;*.png;*.jpeg";
=======
            dataGridView1.Columns["Id"]!.Visible = false;
            dataGridView1.Columns["KategooriaId"]!.Visible = false;
        }

        // ---------- картинки ----------
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)

        private static Image LoadImage(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var tmp = Image.FromStream(fs);
            return new Bitmap(tmp);
        }

        private void NaitaPilti(string? failiNimi)
        {
            toodePilt.Image?.Dispose();
            toodePilt.Image = null;

            if (string.IsNullOrEmpty(failiNimi)) return;

            string full = Path.Combine(_imagesDir, failiNimi);
            if (File.Exists(full))
                toodePilt.Image = LoadImage(full);
        }

        private void KopeeriPilt()
        {
            if (string.IsNullOrEmpty(valitudPilt)) return;

            Directory.CreateDirectory(_imagesDir);
            string sihtkoht = Path.Combine(_imagesDir, Path.GetFileName(valitudPilt));

            if (!string.Equals(Path.GetFullPath(valitudPilt), Path.GetFullPath(sihtkoht),
                               StringComparison.OrdinalIgnoreCase))
            {
<<<<<<< HEAD
                valitudPilt = ofd.FileName;
                NaitaPilti(valitudPilt);
            }
        }

        private void btnLisa_Click(object sender, EventArgs e)
        {
            if (!LoeVorm(toodeEntry.Text, kogusEntry.Text, hindEntry.Text,
                         out int kogus, out float hind, out int katId))
                return;

            try
            {
                var uus = new Toode
                {
                    Toodenimetus = toodeEntry.Text,
                    Kogus = kogus,
                    Hind = hind,
                    KategooriaId = katId,
                    Pilt = KopeeriPilt()
                };

                _context.Toodetabel.Add(uus);
                _context.SaveChanges();

                valitudPilt = null;
                LaeTooted();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga lisamisel: " + ex.GetBaseException().Message);
            }
        }

        private void dataGridViewTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var rida = dataGridView1.Rows[e.RowIndex];

            nimetus.Text = rida.Cells["Toodenimetus"].Value?.ToString();
            kogus.Text = rida.Cells["Kogus"].Value?.ToString();
            hind.Text = rida.Cells["Hind"].Value?.ToString();

            if (rida.Cells["KategooriaId"].Value is int katId)
                kategooriaBox.SelectedValue = katId;

            valitudPilt = null; // uut pilti pole veel valitud

            string pilt = rida.Cells["Pilt"].Value?.ToString();
            NaitaPilti(string.IsNullOrEmpty(pilt) ? null : Path.Combine(PildiKaust, pilt));
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            int? id = ValitudId();
            if (id == null) return;

            if (!LoeVorm(nimetus.Text, kogus.Text, hind.Text,
                         out int uusKogus, out float uusHind, out int katId))
                return;

            try
            {
                var toode = _context.Toodetabel.Find(id.Value);
                if (toode == null) return;

                toode.Toodenimetus = nimetus.Text;
                toode.Kogus = uusKogus;
                toode.Hind = uusHind;
                toode.KategooriaId = katId;

                if (!string.IsNullOrEmpty(valitudPilt))
                    toode.Pilt = KopeeriPilt();

                _context.SaveChanges();

                valitudPilt = null;
                LaeTooted();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga lisamisel: " + ex.GetBaseException().Message);
            }
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            int? id = ValitudId();
            if (id == null) return;
=======
                File.Copy(valitudPilt, sihtkoht, true);
            }
        }

        private void ValiPiltbtn_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Pildid|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                valitudPilt = dlg.FileName;
                toodePilt.Image?.Dispose();
                toodePilt.Image = LoadImage(dlg.FileName);
            }
        }

        // ---------- проверка ввода ----------

        private bool TryReadInput(out int kogus, out float hind, out int kategooriaId)
        {
            kogus = 0;
            hind = 0;
            kategooriaId = 0;

            if (string.IsNullOrWhiteSpace(toodeEntry.Text))
            {
                MessageBox.Show("Sisesta toote nimetus!");
                return false;
            }

            if (!int.TryParse(kogusEntry.Text, out kogus))
            {
                MessageBox.Show("Kogus peab olema täisarv!");
                return false;
            }

            string hindTekst = hindEntry.Text.Replace(',', '.');
            if (!float.TryParse(hindTekst, NumberStyles.Float, CultureInfo.InvariantCulture, out hind))
            {
                MessageBox.Show("Hind peab olema arv!");
                return false;
            }

            if (kategooriaBox.SelectedValue == null)
            {
                MessageBox.Show("Vali kategooria!");
                return false;
            }
            kategooriaId = (int)kategooriaBox.SelectedValue;

            return true;
        }

        // ---------- товары ----------

        private void Lisabtn_Click(object? sender, EventArgs e)
        {
            if (!TryReadInput(out int kogus, out float hind, out int kategooriaId))
                return;

            var uus = new Toode
            {
                Toodenimetus = toodeEntry.Text,
                Kogus = kogus,
                Hind = hind,
                KategooriaId = kategooriaId,
                Pilt = string.IsNullOrEmpty(valitudPilt) ? null : Path.GetFileName(valitudPilt)
            };

            try
            {
                KopeeriPilt();
                _db.Toodetabel.Add(uus);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga: " + (ex.InnerException?.Message ?? ex.Message));
                return;
            }

            LaeTooted();
            PuhastaVorm();
        }

        private void UuendaBtn_Click(object? sender, EventArgs e)
        {
            if (_valitudId == null)
            {
                MessageBox.Show("Vali tabelist toode!");
                return;
            }

            if (!TryReadInput(out int kogus, out float hind, out int kategooriaId))
                return;

            var toode = _db.Toodetabel.Find(_valitudId.Value);
            if (toode == null)
            {
                MessageBox.Show("Toodet ei leitud!");
                return;
            }

            toode.Toodenimetus = toodeEntry.Text;
            toode.Kogus = kogus;
            toode.Hind = hind;
            toode.KategooriaId = kategooriaId;

            // картинку меняем только если выбрали новую
            if (!string.IsNullOrEmpty(valitudPilt))
                toode.Pilt = Path.GetFileName(valitudPilt);

            try
            {
                KopeeriPilt();
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga: " + (ex.InnerException?.Message ?? ex.Message));
                return;
            }

            LaeTooted();
            PuhastaVorm();
        }

        private void KustutaBtn_Click(object? sender, EventArgs e)
        {
            if (_valitudId == null)
            {
                MessageBox.Show("Vali tabelist toode!");
                return;
            }
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)

            if (MessageBox.Show("Kas kustutada toode?", "Kinnitus",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

<<<<<<< HEAD
            var toode = _context.Toodetabel.Find(id.Value);
            if (toode == null) return;

            _context.Toodetabel.Remove(toode);
            _context.SaveChanges();

            NaitaPilti(null);
            LaeTooted();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toodePilt.Image?.Dispose();
            _context.Dispose();
            base.OnFormClosed(e);
        }

        private void btnLisaKategooria_Click(object sender, EventArgs e)
        {
            string nimi = kategooriaEntry.Text.Trim();

            if (string.IsNullOrEmpty(nimi))
            {
                MessageBox.Show("Sisesta kategooria nimetus!");
                return;
            }

            if (_context.Kategooriatabel.Any(k => k.nimetus.ToLower() == nimi.ToLower()))
=======
            var toode = _db.Toodetabel.Find(_valitudId.Value);
            if (toode != null)
            {
                _db.Toodetabel.Remove(toode);
                _db.SaveChanges();
            }

            LaeTooted();
            PuhastaVorm();
        }

        private void PuhastaBtn_Click(object? sender, EventArgs e) => PuhastaVorm();

        private void PuhastaVorm()
        {
            toodeEntry.Clear();
            kogusEntry.Clear();
            hindEntry.Clear();
            if (kategooriaBox.Items.Count > 0) kategooriaBox.SelectedIndex = 0;

            toodePilt.Image?.Dispose();
            toodePilt.Image = null;

            valitudPilt = null;
            _valitudId = null;
            dataGridView1.ClearSelection();
        }

        private void dataGridViewTooted_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dataGridView1.Rows[e.RowIndex];

            _valitudId = Convert.ToInt32(row.Cells["Id"].Value);
            toodeEntry.Text = row.Cells["Toodenimetus"].Value?.ToString();
            kogusEntry.Text = row.Cells["Kogus"].Value?.ToString();
            hindEntry.Text = row.Cells["Hind"].Value?.ToString();

            if (row.Cells["KategooriaId"].Value != null)
                kategooriaBox.SelectedValue = Convert.ToInt32(row.Cells["KategooriaId"].Value);

            valitudPilt = null;
            NaitaPilti(row.Cells["Pilt"].Value?.ToString());
        }

        // ---------- категории ----------

        private void LisaKategooriaBtn_Click(object? sender, EventArgs e)
        {
            string nimi = kategooriaBox.Text.Trim();
            if (string.IsNullOrEmpty(nimi))
            {
                MessageBox.Show("Kirjuta kategooria nimi kategooria lahtrisse!");
                return;
            }

            if (_db.Kategooriatabel.Any(k => k.nimetus == nimi))
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)
            {
                MessageBox.Show("Selline kategooria on juba olemas!");
                return;
            }

<<<<<<< HEAD
            try
            {
                var uus = new Kategooria { nimetus = nimi };
                _context.Kategooriatabel.Add(uus);
                _context.SaveChanges();

                kategooriaEntry.Clear();
                LaeKategooriad();
                kategooriaBox.SelectedValue = uus.Id; // сразу выбираем новую категорию
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga kategooria lisamisel: " + ex.GetBaseException().Message);
            }
        }

        private void btnKustutaKategooria_Click(object sender, EventArgs e)
        {
            if (kategooriaBox.SelectedValue is not int katId)
            {
                MessageBox.Show("Vali kustutatav kategooria nimekirjast!");
                return;
            }

            int tooteid = _context.Toodetabel.Count(t => t.KategooriaId == katId);
            if (tooteid > 0)
            {
                MessageBox.Show($"Kategoorias on {tooteid} toodet. Kustuta või muuda enne need tooted.");
                return;
            }

            if (MessageBox.Show($"Kas kustutada kategooria \"{kategooriaBox.Text}\"?",
                    "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                var kategooria = _context.Kategooriatabel.Find(katId);
                if (kategooria == null) return;

                _context.Kategooriatabel.Remove(kategooria);
                _context.SaveChanges();

                LaeKategooriad();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga kategooria kustutamisel: " + ex.GetBaseException().Message);
            }
        }

        private async void poodBtn_Click(object sender, EventArgs e)
        {
            List<OstuRida> ostud;
            string email;
            decimal kokku;

            using (var pood = new Form3(OtsiPoest))
            {
                if (pood.ShowDialog(this) != DialogResult.OK) return;
                ostud = pood.Ostud;
                email = pood.Email;
                kokku = pood.Kokku;
            }

            // 1) Kontrollime laoseisu uuesti ja vähendame kogust andmebaasis
            foreach (var o in ostud)
            {
                var toode = _context.Toodetabel.Find(o.ToodeId);
                if (toode == null || toode.Kogus < o.Kogus)
                {
                    MessageBox.Show($"Toodet \"{o.Nimetus}\" pole laos piisavalt. Ost tühistati.", "Pood");
                    _context.ChangeTracker.Clear(); // tühistab salvestamata muudatused
                    LaeTooted();
                    return;
                }
                toode.Kogus -= o.Kogus;
            }
            _context.SaveChanges();
            LaeTooted();

            int kokkuTk = ostud.Sum(o => o.Kogus);
            string tooteNimed = string.Join(", ", ostud.Select(o => $"{o.Nimetus} ×{o.Kogus}"));
            ostuInfoLabel.Text = $"Ostetud: {kokkuTk} tk\n{tooteNimed}\nKokku: {kokku:0.00} €";

            // 2) Saadame tšeki e-postile
            DateTime aeg = DateTime.Now;
            string tsekiNr = aeg.ToString("yyyyMMddHHmmss");
            string html = TsekiKoostaja.LooHtml(ostud, tsekiNr, aeg, email);

            try
            {
                poodBtn.Enabled = false;
                Cursor = Cursors.WaitCursor;
                await EpostiSaatja.SaadaAsync(email, $"Teie ostutšekk nr {tsekiNr}", html);
                MessageBox.Show($"Ost õnnestus! Tšekk saadeti aadressile {email}.", "Pood",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ost on tehtud, aga tšeki saatmine ebaõnnestus:\n" + ex.Message, "Pood",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
                poodBtn.Enabled = true;
            }
        }

        /// <summary>Otsib tooteid andmebaasist nime või kategooria järgi (SQL LIKE).</summary>
        private List<PoodToode> OtsiPoest(string otsing)
        {
            var paring = _context.Toodetabel.AsQueryable();

            if (!string.IsNullOrWhiteSpace(otsing))
            {
                paring = paring.Where(t =>
                    t.Toodenimetus.Contains(otsing) ||
                    t.Kategooria.nimetus.Contains(otsing));
            }

            return paring
                .OrderBy(t => t.Toodenimetus)
                .Select(t => new PoodToode
                {
                    Id = t.Id,
                    Nimetus = t.Toodenimetus,
                    Kogus = (int)t.Kogus,
                    Hind = (decimal)t.Hind,
                    Kategooria = t.Kategooria.nimetus,
                    Pilt = t.Pilt
                })
                .ToList();
=======
            _db.Kategooriatabel.Add(new Kategooria { nimetus = nimi });
            _db.SaveChanges();

            LaeKategooriad();
            kategooriaBox.SelectedIndex = kategooriaBox.FindStringExact(nimi);
        }

        private void KustutaKategooriaBtn_Click(object? sender, EventArgs e)
        {
            if (kategooriaBox.SelectedValue == null)
            {
                MessageBox.Show("Vali kategooria!");
                return;
            }

            int id = (int)kategooriaBox.SelectedValue;

            if (_db.Toodetabel.Any(t => t.KategooriaId == id))
            {
                MessageBox.Show("Kategooriat ei saa kustutada, selles on tooteid!");
                return;
            }

            var kat = _db.Kategooriatabel.Find(id);
            if (kat != null)
            {
                _db.Kategooriatabel.Remove(kat);
                _db.SaveChanges();
            }

            LaeKategooriad();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toodePilt.Image?.Dispose();
            _db.Dispose();
            base.OnFormClosed(e);
        }



        // Ostukorv: valitud tooted poe aknast
        private List<Toode> _korv = new();

        // "Pood" nupp: avab poe akna; pärast sulgemist uuendame tabeli (laoseis muutus)
        private void PoodBtn_Click(object? sender, EventArgs e)
        {
            using var pood = new PoodForm();
            pood.ShowDialog(this);
            LaeTooted();
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)
        }
    }
}