using Microsoft.EntityFrameworkCore;

namespace Tooded_2
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            Shop.T.SetLanguage(Shop.T.Language);

            if (args.Contains("--demo", StringComparer.OrdinalIgnoreCase))
            {
                Application.Run(new PoodForm(new Shop.OnlineDemoShopStore()));
                return;
            }

            // Kui andmebaasis pole admini, loome vaikimisi admini
            using (var db = new AppDbContext())
            {
                db.Database.Migrate();
                if (!db.Kasutajatabel.Any(k => k.Roll == "Admin"))
                {
                    db.Kasutajatabel.Add(new Kasutaja
                    {
                        Kasutajanimi = "admin",
                        ParoolHash = Parool.Hashi("admin123"),
                        Roll = "Admin"
                    });
                    db.SaveChanges();
                }
            }

            // Sisselogimise tsükkel: pärast akna sulgemist naaseme sisselogimisse
            while (true)
            {
                using var login = new LoginForm();
                if (login.ShowDialog() != DialogResult.OK) break;   // login suletud = väljumine

                // Admin näeb toodete vormi, tavakasutaja kohe poodi
                Form avatav = login.Roll == "Admin" ? new Form2() : new PoodForm();
                Application.Run(avatav);
                avatav.Dispose();
            }
        }
    }
}
