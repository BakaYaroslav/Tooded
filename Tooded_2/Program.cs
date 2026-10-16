namespace Tooded_2
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Kui andmebaasis pole admini, loome vaikimisi admini
            using (var db = new AppDbContext())
            {
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