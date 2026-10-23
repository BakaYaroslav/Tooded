using System.Globalization;
using System.Net;
using System.Text;

namespace Tooded_2
{
    /// <summary>Koostab ostutšeki HTML-kujul (e-kirja sisuks).</summary>
    public static class TsekiKoostaja
    {
        // ---- Poe andmed (muuda enda omadeks) ----
        public const string PoeNimi = "Shoping 67";
        public const string Ettevote = "Shoping 67 OÜ";
        public const string RegNr = "12345678";
        public const string Aadress = "Tallinn";
        public const string Telefon = "+372 5000 0000";
        public const string Epost = "info@shoping67.ee";

        // Käibemaks Eestis (alates 01.07.2025 on 24%)
        public const decimal KmMaar = 24m;

        private static readonly CultureInfo Et = new("et-EE");

        private static string Raha(decimal x) => x.ToString("0.00", Et) + " €";
        private static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

        public static string LooHtml(List<OstuRida> ostud, string tsekiNr, DateTime aeg, string klient)
        {
            decimal summa = ostud.Sum(o => o.Summa);
            decimal ilmaKm = Math.Round(summa / (1 + KmMaar / 100m), 2);
            decimal km = summa - ilmaKm;
            int tykke = ostud.Sum(o => o.Kogus);

            var sb = new StringBuilder();

            // Päis: ettevõtte nimi
            sb.Append($@"
<div style=""background:#f4f4f4;padding:24px 0;font-family:Georgia,'Times New Roman',serif;"">
<table cellpadding=""0"" cellspacing=""0"" align=""center""
       style=""width:600px;max-width:100%;background:#ffffff;border:1px solid #dddddd;color:#222222;font-size:14px;"">
  <tr><td style=""height:6px;background:#2e7d32;font-size:0;line-height:0;"">&nbsp;</td></tr>
  <tr><td style=""padding:32px 40px 8px;"">
    <div style=""font-size:30px;font-weight:bold;color:#111111;"">{H(Ettevote)}</div>
  </td></tr>

  <tr><td style=""padding:20px 40px 8px;"">
    <table cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;"">
      <tr><td style=""padding:2px 14px 2px 0;font-weight:bold;"">Arve nr.:</td><td>{H(tsekiNr)}</td></tr>
      <tr><td style=""padding:2px 14px 2px 0;font-weight:bold;"">Kuupäev:</td><td>{aeg:dd.MM.yyyy HH:mm}</td></tr>
      <tr><td style=""padding:2px 14px 2px 0;font-weight:bold;"">Staatus:</td><td style=""color:#2e7d32;font-weight:bold;"">Makstud</td></tr>
    </table>
  </td></tr>

  <tr><td style=""padding:16px 40px 8px;"">
    <table cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;"">
      <tr><td style=""padding:2px 14px 2px 0;font-weight:bold;"">Maksja:</td><td>{H(klient)}</td></tr>
    </table>
  </td></tr>

  <tr><td style=""padding:24px 40px 0;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;"">
      <tr style=""font-weight:bold;"">
        <td style=""padding:0 0 8px;border-bottom:2px solid #555555;"">Toode</td>
        <td style=""padding:0 0 8px;border-bottom:2px solid #555555;text-align:center;"">Kogus</td>
        <td style=""padding:0 0 8px;border-bottom:2px solid #555555;text-align:right;"">Hind</td>
        <td style=""padding:0 0 8px;border-bottom:2px solid #555555;text-align:right;"">Summa</td>
      </tr>");

            // Tooted
            foreach (var o in ostud)
            {
                sb.Append($@"
      <tr>
        <td style=""padding:10px 0;border-bottom:1px solid #eeeeee;"">{H(o.Nimetus)}</td>
        <td style=""padding:10px 0;border-bottom:1px solid #eeeeee;text-align:center;"">{o.Kogus}</td>
        <td style=""padding:10px 0;border-bottom:1px solid #eeeeee;text-align:right;white-space:nowrap;"">{Raha(o.Hind)}</td>
        <td style=""padding:10px 0;border-bottom:1px solid #eeeeee;text-align:right;white-space:nowrap;"">{Raha(o.Summa)}</td>
      </tr>");
            }

            // Kokkuvõte ja jalus
            sb.Append($@"
    </table>
  </td></tr>

  <tr><td style=""padding:16px 40px 0;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;"">
      <tr>
        <td style=""padding:3px 0;text-align:right;color:#555555;"">Summa ilma KM-ta:</td>
        <td style=""padding:3px 0 3px 20px;text-align:right;width:110px;white-space:nowrap;"">{Raha(ilmaKm)}</td>
      </tr>
      <tr>
        <td style=""padding:3px 0;text-align:right;color:#555555;"">Käibemaks {KmMaar:0}%:</td>
        <td style=""padding:3px 0 3px 20px;text-align:right;white-space:nowrap;"">{Raha(km)}</td>
      </tr>
      <tr>
        <td style=""padding:10px 0 3px;text-align:right;font-size:16px;font-weight:bold;"">KOKKU:</td>
        <td style=""padding:10px 0 3px 20px;text-align:right;font-size:16px;font-weight:bold;white-space:nowrap;"">{Raha(summa)}</td>
      </tr>
      <tr>
        <td style=""padding:3px 0;text-align:right;color:#555555;font-size:12px;"">Tooteid kokku:</td>
        <td style=""padding:3px 0 3px 20px;text-align:right;font-size:12px;color:#555555;"">{tykke} tk</td>
      </tr>
    </table>
  </td></tr>

  <tr><td style=""padding:28px 40px 24px;font-style:italic;color:#444444;"">Aitäh ostu eest!</td></tr>

  <tr><td style=""padding:0 40px 28px;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0""
           style=""border-top:1px solid #999999;font-size:11px;color:#444444;"">
      <tr style=""vertical-align:top;"">
        <td style=""padding-top:10px;width:34%;"">{H(Ettevote)}<br>{H(Aadress)}</td>
        <td style=""padding-top:10px;width:36%;"">Telefon: {H(Telefon)}<br>E-post: {H(Epost)}<br>Reg. nr: {H(RegNr)}</td>
        <td style=""padding-top:10px;"">{H(PoeNimi)}</td>
      </tr>
    </table>
  </td></tr>
</table>
</div>");

            return sb.ToString();
        }
    }
}