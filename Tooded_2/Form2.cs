using Tooded_2.Shop;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Tooded_2
{
    public partial class Form2 : Form
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly string _imagesDir = Path.Combine(AppContext.BaseDirectory, "Images");

        private string? valitudPilt;
        private int? _valitudId;

        public Form2()
        {
            InitializeComponent();
            foreach (Control control in Controls) control.Top += 48;
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + 48);
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 4, 0, 0), BackColor = Color.White };
            toolbar.Controls.Add(Shop67Ui.LanguagePicker(() => { Shop67Ui.Translate(this); LaeMuuk(); }));
            var shopManagement = Shop67Ui.Button(T.L("Kampaaniad ja omadused", "Акции и характеристики", "Promotions and specifications"), () =>
            {
                using var editor = new ShopAdminForm(); editor.ShowDialog(this); LaeTooted(); LaeKategooriad(); LaeMuuk();
            });
            shopManagement.Width = 310; toolbar.Controls.Add(shopManagement);
            Controls.Add(toolbar);
            Shop67Ui.Translate(this);


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
            LaeKategooriad();
            LaeTooted();
        }


        private void LaeKategooriad()
        {
            var kategooriad = _db.Kategooriatabel.AsNoTracking().ToList();

            kategooriaBox.DataSource = null;
            kategooriaBox.DisplayMember = "nimetus";
            kategooriaBox.ValueMember = "Id";
            kategooriaBox.DataSource = kategooriad;
            kategooriaBox.SelectedIndex = kategooriad.Count > 0 ? 0 : -1;
        }

        private void LaeTooted()
        {
            dataGridView1.DataSource = _db.Toodetabel
                .AsNoTracking()
                .Select(t => new
                {
                    t.Id,
                    t.Toodenimetus,
                    t.Kogus,
                    t.Hind,
                    t.KategooriaId,
                    t.Pilt
                })
                .ToList();

            dataGridView1.Columns["Id"]!.Visible = false;
            dataGridView1.Columns["KategooriaId"]!.Visible = false;
        }

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


        private bool TryReadInput(out int kogus, out float hind, out int kategooriaId)
        {
            kogus = 0;
            hind = 0;
            kategooriaId = 0;

            if (string.IsNullOrWhiteSpace(toodeEntry.Text))
            {
                MessageBox.Show(T.Legacy("Sisesta toote nimetus!"));
                return false;
            }

            if (!int.TryParse(kogusEntry.Text, out kogus))
            {
                MessageBox.Show(T.Legacy("Kogus peab olema täisarv!"));
                return false;
            }

            string hindTekst = hindEntry.Text.Replace(',', '.');
            if (!float.TryParse(hindTekst, NumberStyles.Float, CultureInfo.InvariantCulture, out hind))
            {
                MessageBox.Show(T.Legacy("Hind peab olema arv!"));
                return false;
            }

            if (kategooriaBox.SelectedValue == null)
            {
                MessageBox.Show(T.Legacy("Vali kategooria!"));
                return false;
            }
            kategooriaId = (int)kategooriaBox.SelectedValue;

            return true;
        }


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
                MessageBox.Show(T.Legacy("Viga: ") + (ex.InnerException?.Message ?? ex.Message));
                return;
            }

            LaeTooted();
            PuhastaVorm();
        }

        private void UuendaBtn_Click(object? sender, EventArgs e)
        {
            if (_valitudId == null)
            {
                MessageBox.Show(T.Legacy("Vali tabelist toode!"));
                return;
            }

            if (!TryReadInput(out int kogus, out float hind, out int kategooriaId))
                return;

            var toode = _db.Toodetabel.Find(_valitudId.Value);
            if (toode == null)
            {
                MessageBox.Show(T.Legacy("Toodet ei leitud!"));
                return;
            }

            toode.Toodenimetus = toodeEntry.Text;
            toode.Kogus = kogus;
            toode.Hind = hind;
            toode.KategooriaId = kategooriaId;

            if (!string.IsNullOrEmpty(valitudPilt))
                toode.Pilt = Path.GetFileName(valitudPilt);

            try
            {
                KopeeriPilt();
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(T.Legacy("Viga: ") + (ex.InnerException?.Message ?? ex.Message));
                return;
            }

            LaeTooted();
            PuhastaVorm();
        }

        private void KustutaBtn_Click(object? sender, EventArgs e)
        {
            if (_valitudId == null)
            {
                MessageBox.Show(T.Legacy("Vali tabelist toode!"));
                return;
            }

            if (MessageBox.Show(T.Legacy("Kas kustutada toode?"), T.Legacy("Kinnitus"),
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

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
            kogusEntry.Text = row.Cells[T.Legacy("Kogus")].Value?.ToString();
            hindEntry.Text = row.Cells["Hind"].Value?.ToString();

            if (row.Cells["KategooriaId"].Value != null)
                kategooriaBox.SelectedValue = Convert.ToInt32(row.Cells["KategooriaId"].Value);

            valitudPilt = null;
            NaitaPilti(row.Cells["Pilt"].Value?.ToString());
        }


        private void LisaKategooriaBtn_Click(object? sender, EventArgs e)
        {
            string nimi = kategooriaBox.Text.Trim();
            if (string.IsNullOrEmpty(nimi))
            {
                MessageBox.Show(T.Legacy("Kirjuta kategooria nimi kategooria lahtrisse!"));
                return;
            }

            if (_db.Kategooriatabel.Any(k => k.nimetus == nimi))
            {
                MessageBox.Show(T.Legacy("Selline kategooria on juba olemas!"));
                return;
            }

            _db.Kategooriatabel.Add(new Kategooria { nimetus = nimi });
            _db.SaveChanges();

            LaeKategooriad();
            kategooriaBox.SelectedIndex = kategooriaBox.FindStringExact(nimi);
        }

        private void KustutaKategooriaBtn_Click(object? sender, EventArgs e)
        {
            if (kategooriaBox.SelectedValue == null)
            {
                MessageBox.Show(T.Legacy("Vali kategooria!"));
                return;
            }

            int id = (int)kategooriaBox.SelectedValue;

            if (_db.Toodetabel.Any(t => t.KategooriaId == id))
            {
                MessageBox.Show(T.Legacy("Kategooriat ei saa kustutada, selles on tooteid!"));
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

        // T.Legacy("Pood") nupp: avab poe akna; pärast sulgemist uuendame tabeli (laoseis muutus)
        private void PoodBtn_Click(object? sender, EventArgs e)
        {
            using var pood = new PoodForm();
            pood.ShowDialog(this);
            LaeTooted();
            LaeMuuk();
        }

        private void LaeMuuk()
        {
            int tykke = _db.Muugitabel.Sum(m => (int?)m.Kogus) ?? 0;
            decimal summa = _db.Muugitabel.Sum(m => (decimal?)m.Summa) ?? 0m;
            muukLabel.Text = string.Format(T.L("Müüdud: {0} tk | Tulu kokku: {1}", "Продано: {0} шт. | Выручка: {1}", "Sold: {0} | Revenue: {1}"), tykke, T.Money(summa));
        }
    }
}