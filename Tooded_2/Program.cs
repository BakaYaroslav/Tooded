using Microsoft.EntityFrameworkCore;

namespace Tooded_2;
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        LanguageManager.Initialize();
        ApplicationConfiguration.Initialize();
        // Служебные режимы используются проверками GitHub Actions.
        if (args.Length == 2 && args[0] == "--language-settings-check")
        {
            Environment.ExitCode = LanguageManager.Language == args[1] ? 0 : 1;
            return;
        }

        if (args.Contains("--ui-check"))
        {
            ShopUiChecks.Run();
            return;
        }

        if (args.Contains("--bundle-catalog-images"))
        {
            CatalogPhotos.Bundle().GetAwaiter().GetResult();
            return;
        }

        if (args.Contains("--demo"))
        {
            Application.Run(new StoreForm(new DemoShopStore()));
            return;
        }

        PrepareDatabase();
        ShowLoginAndMainWindow();
    }

    private static void PrepareDatabase()
    {
        using var database = new AppDbContext();
        database.Database.Migrate();
        if (database.Kasutajatabel.Any(user => user.Roll == "Admin"))
            return;
        database.Kasutajatabel.Add(new Kasutaja { Kasutajanimi = "admin", ParoolHash = Parool.Hashi("admin123"), Roll = "Admin" });
        database.SaveChanges();
    }

    private static void ShowLoginAndMainWindow()
    {
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK)
                return;
            using Form mainWindow = login.Roll == "Admin" ? new AdminForm() : new StoreForm();
            Application.Run(mainWindow);
        }
    }
}
