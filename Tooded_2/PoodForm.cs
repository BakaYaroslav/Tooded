using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace Tooded_2
{
    public partial class PoodForm : Form
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly string _imagesDir = Path.Combine(AppContext.BaseDirectory, "Images");

        private readonly List<Toode> _tooted;                              // kõik tooted andmebaasist
        private readonly Dictionary<int, int> _kogused = new();            // toote Id -> valitud kogus

        private List<(Toode Toode, int Kogus)> _ostukorv = new();          // fikseeritud ostukorv
        private decimal _kokku;                                            // ostukorvi kogusumma
        private int _etapp;                                                // 0 = valik, 1 = ostetud, 2 = makstud

        public PoodForm()
        {
            InitializeComponent();

            // Laeme tooted koos kategooriatega
            _tooted = _db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).ToList();

            // Täidame kategooriate rippmenüü
            kategooriaBox.Items.Add("Kõik");
            foreach (var nimi in _tooted.Select(t => t.Kategooria?.nimetus)
                                        .Where(n => n != null).Distinct())
            {
                kategooriaBox.Items.Add(nimi!);
            }
            kategooriaBox.SelectedIndex = 0;   // see kutsub ka Kuva()
        }

        // ---------- toodete kuvamine ----------

        // Joonistab kaardid uuesti vastavalt otsingule ja kategooriale
        private void Kuva()
        {
            flowPanel.SuspendLayout();

            // Vabastame vanad kaardid ja nende pildid
            foreach (Control c in flowPanel.Controls.Cast<Control>().ToList())
            {
                foreach (var pb in c.Controls.OfType<PictureBox>())
                    pb.Image?.Dispose();
                c.Dispose();
            }

            string otsing = otsingEntry.Text.Trim();
            string kat = kategooriaBox.SelectedItem as string ?? "Kõik";

            // Filtreerime nime ja kategooria järgi
            var nahtav = _tooted.Where(t =>
                (kat == "Kõik" || t.Kategooria?.nimetus == kat) &&
                (otsing == "" || t.Toodenimetus.Contains(otsing, StringComparison.OrdinalIgnoreCase)));

            foreach (var t in nahtav)
                flowPanel.Controls.Add(LooKaart(t));

            flowPanel.ResumeLayout();
            UuendaKokku();
        }

        // Loob ühe toote kaardi (pilt, nimi, hind, laoseis, kogus)
        private Panel LooKaart(Toode t)
        {
            bool laos = t.Kogus > 0;
            _kogused.TryGetValue(t.Id, out int algKogus);

            var kaart = new Panel
            {
                Size = new Size(190, 285),
                Margin = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = laos ? Cursors.Hand : Cursors.Default,
                Tag = laos   // jätame meelde, kas toode on laos
            };

            var pb = new PictureBox
            {
                Location = new Point(14, 10),
                Size = new Size(160, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = LaePilt(t.Pilt)
            };

            var nimi = new Label
            {
                Text = t.Toodenimetus,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 138),
                Size = new Size(168, 24),
                AutoEllipsis = true
            };

            var hind = new Label
            {
                Text = $"{t.Hind:0.00} €",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.SeaGreen,
                Location = new Point(10, 164),
                Size = new Size(168, 26)
            };

            var info = new Label
            {
                Text = laos
                    ? $"{t.Kategooria?.nimetus}  •  Laos: {t.Kogus}"
                    : $"{t.Kategooria?.nimetus}  •  Otsas",
                ForeColor = laos ? Color.Gray : Color.IndianRed,
                Location = new Point(10, 194),
                Size = new Size(168, 36)
            };

            var kogusLabel = new Label
            {
                Text = "Kogus:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(10, 243),
                AutoSize = true
            };

            // Koguse valik: 0 kuni laoseis
            var kogusNum = new NumericUpDown
            {
                Name = "kogusNum",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(75, 239),
                Size = new Size(70, 25),
                Minimum = 0,
                Maximum = laos ? t.Kogus : 0,
                Value = Math.Min(algKogus, laos ? t.Kogus : 0),
                Enabled = laos
            };

            // Linnuke, mis ilmub valitud kaardile
            var linnuke = new Label
            {
                Name = "linnuke",
                Text = "✔",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.SeaGreen,
                Location = new Point(158, 2),
                AutoSize = true,
                Visible = false
            };

            kaart.Controls.AddRange(new Control[] { linnuke, pb, nimi, hind, info, kogusLabel, kogusNum });

            // Koguse muutmisel jätame kogused meelde ja värvime kaardi
            kogusNum.ValueChanged += (s, e) =>
            {
                int v = (int)kogusNum.Value;
                if (v > 0) _kogused[t.Id] = v; else _kogused.Remove(t.Id);

                VarviKaart(kaart, v > 0);
                UuendaKokku();
            };

            // Klõps kaardil: kui kogus on 0, paneb 1; muidu nullib
            if (laos)
            {
                void Klops(object? s, EventArgs e) =>
                    kogusNum.Value = kogusNum.Value > 0 ? 0 : 1;

                kaart.Click += Klops;
                foreach (Control c in kaart.Controls)
                    if (c != kogusNum) c.Click += Klops;
            }

            VarviKaart(kaart, kogusNum.Value > 0);
            return kaart;
        }

        // Värvib kaardi: roheline = valitud, hall = otsas, valge = tavaline
        private static void VarviKaart(Panel kaart, bool valitud)
        {
            bool laos = (bool)kaart.Tag!;

            if (valitud) kaart.BackColor = Color.FromArgb(210, 240, 210);
            else kaart.BackColor = laos ? Color.White : Color.Gainsboro;

            kaart.Controls["linnuke"]!.Visible = valitud;
        }

        // Arvutab valitud toodete kogusumma
        private decimal ArvutaSumma(IEnumerable<(Toode Toode, int Kogus)> read) =>
            read.Sum(r => (decimal)r.Toode.Hind * r.Kogus);

        // Valitud tooted koos kogustega
        private List<(Toode Toode, int Kogus)> ValitudRead() =>
            _tooted.Where(t => _kogused.ContainsKey(t.Id))
                   .Select(t => (t, _kogused[t.Id]))
                   .ToList();

        // Uuendab alumist teksti
        private void UuendaKokku()
        {
            if (_etapp > 0)
            {
                kokkuLabel.Text = $"Ostukorv: {_ostukorv.Sum(r => r.Kogus)} tk   |   Kokku: {_kokku:0.00} €";
                return;
            }

            var v = ValitudRead();
            kokkuLabel.Text = $"Valitud: {v.Sum(r => r.Kogus)} tk   |   Kokku: {ArvutaSumma(v):0.00} €";
        }

        // ---------- pildid ----------

        // Laeb pildi ilma faili lukustamata
        private Image? LaePilt(string? failiNimi)
        {
            if (string.IsNullOrEmpty(failiNimi)) return null;

            string full = Path.Combine(_imagesDir, failiNimi);
            if (!File.Exists(full)) return null;

            using var fs = new FileStream(full, FileMode.Open, FileAccess.Read);
            using var tmp = Image.FromStream(fs);
            return new Bitmap(tmp);
        }

        // ---------- otsing ja filter ----------

        private void OtsingEntry_TextChanged(object? sender, EventArgs e) => Kuva();

        private void KategooriaBox_SelectedIndexChanged(object? sender, EventArgs e) => Kuva();

        // ---------- ostuprotsess ----------

        // "Ostan": fikseerib ostukorvi ja lukustab valiku
        private void OstanBtn_Click(object? sender, EventArgs e)
        {
            var valitud = ValitudRead();
            if (valitud.Count == 0)
            {
                MessageBox.Show("Vali vähemalt üks toode!");
                return;
            }

            _ostukorv = valitud;
            _kokku = ArvutaSumma(valitud);
            _etapp = 1;

            flowPanel.Enabled = false;     // valikut enam muuta ei saa
            ostanBtn.Enabled = false;
            makstaBtn.Enabled = true;
            UuendaKokku();
        }

        // "Maksta": kinnitab makse ja vähendab laoseisu andmebaasis
        private void MakstaBtn_Click(object? sender, EventArgs e)
        {
            if (_etapp != 1) return;

            if (MessageBox.Show($"Kogusumma on {_kokku:0.00} €. Kas maksad?", "Makse",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                foreach (var (toode, kogus) in _ostukorv)
                {
                    var db = _db.Toodetabel.Find(toode.Id);
                    if (db == null || db.Kogus < kogus)
                    {
                        MessageBox.Show($"Toodet '{toode.Toodenimetus}' ei ole piisavalt laos!");
                        return;
                    }
                }

                DateTime aeg = DateTime.Now;
                foreach (var (toode, kogus) in _ostukorv)
                {
                    _db.Toodetabel.Find(toode.Id)!.Kogus -= kogus;

                    _db.Muugitabel.Add(new Muuk
                    {
                        Aeg = aeg,
                        ToodeId = toode.Id,
                        Toodenimetus = toode.Toodenimetus,
                        Kogus = kogus,
                        Hind = (decimal)toode.Hind,
                        Summa = (decimal)toode.Hind * kogus
                    });
                }

                _db.SaveChanges();

                foreach (var (toode, kogus) in _ostukorv)
                    toode.Kogus -= kogus;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga: " + (ex.InnerException?.Message ?? ex.Message));
                return;
            }

            _etapp = 2;
            makstaBtn.Enabled = false;
            saadaArveBtn.Enabled = true;
            MessageBox.Show("Makse õnnestus!");
        }

        // "Saada arve": loob arve faili, avab selle ja alustab uut ostu
        // Gmaili andmed (parool = Google'i "rakenduse parool", mitte tavaline parool!)
        private const string SaatjaEmail = "jaroslavbaka25@gmail.com";
        private const string SaatjaParool = "txse crlz zdut bicm";

        // "Saada arve": loob PDF-i, küsib e-maili, saadab arve ja alustab uut ostu
        // "Saada arve": küsib e-maili, loob PDF-i, saadab arve ja alustab uut ostu
        private void SaadaArveBtn_Click(object? sender, EventArgs e)
        {
            if (_etapp != 2) return;

            string email = Microsoft.VisualBasic.Interaction.InputBox(
                "Sisesta saaja e-mail", "Saada arve", "nimi@example.com").Trim();

            string pdfTee;
            try
            {
                pdfTee = LooPdf(email);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga PDF-i loomisel: " + ex.Message);
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
                MessageBox.Show($"PDF on salvestatud:\n{pdfTee}");
            else if (SaadaEmail(email, pdfTee))
                MessageBox.Show("Arve saadetud!");

            UusOst();
        }

        // Loob arve PDF-faili kausta Dokumendid\Arved ja tagastab faili tee
        private string LooPdf(string klient)
        {
            var et = new System.Globalization.CultureInfo("et-EE");
            string Raha(decimal x) => x.ToString("0.00", et) + " €";

            string kaust = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arved");
            Directory.CreateDirectory(kaust);

            DateTime aeg = DateTime.Now;
            string arveNr = aeg.ToString("yyyyMMddHHmmss");
            string fail = Path.Combine(kaust, $"Arve_{arveNr}.pdf");

            decimal ilmaKm = Math.Round(_kokku / (1 + TsekiKoostaja.KmMaar / 100m), 2);
            decimal km = _kokku - ilmaKm;

            var dokument = new PdfDocument();
            PdfPage leht = dokument.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(leht);

            var nimiFont = new XFont("Arial", 24, XFontStyle.Bold);
            var tekstFont = new XFont("Arial", 11);
            var paksFont = new XFont("Arial", 11, XFontStyle.Bold);
            var vaikeFont = new XFont("Arial", 9);

            double vasak = 40;
            double parem = leht.Width - 40;

            // Ettevõtte nimi
            gfx.DrawString(TsekiKoostaja.Ettevote, nimiFont, XBrushes.Black, new XPoint(vasak, 60));

            // Arve andmed
            double y = 110;
            gfx.DrawString("Arve nr.:", paksFont, XBrushes.Black, new XPoint(vasak, y));
            gfx.DrawString(arveNr, tekstFont, XBrushes.Black, new XPoint(vasak + 70, y));
            y += 16;
            gfx.DrawString("Kuupäev:", paksFont, XBrushes.Black, new XPoint(vasak, y));
            gfx.DrawString($"{aeg:dd.MM.yyyy HH:mm}", tekstFont, XBrushes.Black, new XPoint(vasak + 70, y));
            y += 16;
            gfx.DrawString("Staatus:", paksFont, XBrushes.Black, new XPoint(vasak, y));
            gfx.DrawString("Makstud", tekstFont, XBrushes.Black, new XPoint(vasak + 70, y));

          
            // Tabeli päis
            y += 34;
            gfx.DrawString("Toode", paksFont, XBrushes.Black, new XPoint(vasak, y));
            gfx.DrawString("Kogus", paksFont, XBrushes.Black, new XPoint(330, y));
            gfx.DrawString("Hind", paksFont, XBrushes.Black, new XPoint(400, y));
            gfx.DrawString("Summa", paksFont, XBrushes.Black,
                new XRect(parem - 80, y - 11, 80, 14), XStringFormats.TopRight);
            y += 8;
            gfx.DrawLine(XPens.Gray, vasak, y, parem, y);
            y += 20;

            // Read
            foreach (var (toode, kogus) in _ostukorv)
            {
                decimal rida = (decimal)toode.Hind * kogus;
                string nimi = toode.Toodenimetus;
                if (nimi.Length > 38) nimi = nimi.Substring(0, 35) + "...";

                gfx.DrawString(nimi, tekstFont, XBrushes.Black, new XPoint(vasak, y));
                gfx.DrawString(kogus.ToString(), tekstFont, XBrushes.Black, new XPoint(330, y));
                gfx.DrawString(Raha((decimal)toode.Hind), tekstFont, XBrushes.Black, new XPoint(400, y));
                gfx.DrawString(Raha(rida), tekstFont, XBrushes.Black,
                    new XRect(parem - 90, y - 11, 90, 14), XStringFormats.TopRight);
                y += 22;

                if (y > 640)
                {
                    leht = dokument.AddPage();
                    gfx = XGraphics.FromPdfPage(leht);
                    y = 60;
                }
            }

            // Kokkuvõte
            y += 4;
            gfx.DrawLine(XPens.Gray, vasak, y, parem, y);
            y += 20;
            gfx.DrawString("Summa ilma KM-ta:", tekstFont, XBrushes.Black, new XPoint(350, y));
            gfx.DrawString(Raha(ilmaKm), tekstFont, XBrushes.Black,
                new XRect(parem - 90, y - 11, 90, 14), XStringFormats.TopRight);
            y += 18;
            gfx.DrawString($"Käibemaks {TsekiKoostaja.KmMaar:0}%:", tekstFont, XBrushes.Black, new XPoint(350, y));
            gfx.DrawString(Raha(km), tekstFont, XBrushes.Black,
                new XRect(parem - 90, y - 11, 90, 14), XStringFormats.TopRight);
            y += 22;
            gfx.DrawString("KOKKU:", paksFont, XBrushes.Black, new XPoint(350, y));
            gfx.DrawString(Raha(_kokku), paksFont, XBrushes.Black,
                new XRect(parem - 90, y - 11, 90, 14), XStringFormats.TopRight);

            y += 40;
            gfx.DrawString("Aitäh ostu eest!", tekstFont, XBrushes.Black, new XPoint(vasak, y));

            // Jalus
            double jy = leht.Height - 70;
            gfx.DrawLine(XPens.Gray, vasak, jy - 12, parem, jy - 12);
            gfx.DrawString(TsekiKoostaja.Ettevote, vaikeFont, XBrushes.Black, new XPoint(vasak, jy));
            gfx.DrawString(TsekiKoostaja.Aadress, vaikeFont, XBrushes.Black, new XPoint(vasak, jy + 12));
            gfx.DrawString($"Reg. nr: {TsekiKoostaja.RegNr}", vaikeFont, XBrushes.Black, new XPoint(250, jy));
            gfx.DrawString(TsekiKoostaja.PoeNimi, vaikeFont, XBrushes.Black, new XPoint(250, jy + 12));

            dokument.Save(fail);
            return fail;
        }
      

        // Saadab PDF-i e-mailile Gmaili SMTP kaudu; tagastab true, kui õnnestus
        private bool SaadaEmail(string saaja, string pdfTee)
        {
            try
            {
                using var kiri = new MailMessage();
                kiri.From = new MailAddress(SaatjaEmail);
                kiri.To.Add(saaja);
                kiri.Subject = "Arve";
                kiri.Body = "Aitäh ostu eest! Arve on manuses.";
                kiri.Attachments.Add(new Attachment(pdfTee));

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(SaatjaEmail, SaatjaParool),
                    EnableSsl = true
                };

                smtp.Send(kiri);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("E-maili saatmine ebaõnnestus: " + ex.Message +
                                $"\n\nPDF on siiski salvestatud:\n{pdfTee}");
                return false;
            }
        }

        // Lähtestab poe uue ostu jaoks
        private void UusOst()
        {
            _etapp = 0;
            _ostukorv.Clear();
            _kogused.Clear();
            _kokku = 0;

            flowPanel.Enabled = true;
            ostanBtn.Enabled = true;
            makstaBtn.Enabled = false;
            saadaArveBtn.Enabled = false;

            Kuva();
        }

        // "Sulge": sulgeb poe akna
        private void SulgeBtn_Click(object? sender, EventArgs e) => Close();

        // Akna sulgemisel vabastame pildid ja andmebaasi ühenduse
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (var pb in flowPanel.Controls.Cast<Control>()
                                        .SelectMany(c => c.Controls.OfType<PictureBox>()))
                pb.Image?.Dispose();

            _db.Dispose();
            base.OnFormClosed(e);
        }

    }
}