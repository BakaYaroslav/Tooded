using Microsoft.EntityFrameworkCore;

namespace Tooded_2;
/// <summary>Создание и редактирование товара. Форма собирает ввод, этот класс сохраняет его в БД.</summary>
public static class ProductService
{
    public static int Save(int? productId, ProductDetails details, decimal price, int stock, string image)
    {
        using var database = new AppDbContext();
        return Save(database, productId, details, price, stock, image);
    }

    public static int Save(AppDbContext database, int? productId, ProductDetails details, decimal price, int stock, string image)
    {
        if (string.IsNullOrWhiteSpace(details.Name.Et))
            throw new InvalidOperationException(UiText.Get("EnterAProductName"));
        if (price < 0 || price > 1000000 || stock < 0 || stock > 1000000)
            throw new InvalidOperationException(UiText.Get("InvalidPriceOrStock"));
        if (!CatalogRules.IsTechnical(new ShopProduct { Name = details.Name.Et, Details = details }))
            throw new InvalidOperationException(UiText.Get("ProductModelInvalid"));
        if (details.Visible && string.IsNullOrWhiteSpace(image))
            throw new InvalidOperationException(UiText.Get("AVisibleProductMustHaveAnImage"));
        using var transaction = database.Database.BeginTransaction();
        var configuration = ShopSettingsStore.Load(database);
        Toode product;
        if (productId.HasValue)
        {
            product = database.Toodetabel.Find(productId.Value) ?? throw new InvalidOperationException(UiText.Get("ProductNotFound"));
        }
        else
        {
            product = new Toode();
            database.Toodetabel.Add(product);
        }

        var category = database.Kategooriatabel.FirstOrDefault(item => item.nimetus == details.Category);
        if (category == null)
        {
            category = new Kategooria
            {
                nimetus = details.Category
            };
            database.Kategooriatabel.Add(category);
        }

        product.Kategooria = category;
        product.Toodenimetus = details.Name.Et;
        product.Hind = (float)price;
        product.Kogus = stock;
        product.Pilt = image.Trim();
        database.SaveChanges(); // При добавлении SQL выдаёт Id для связи с характеристиками.
        string sourceKey = details.SourceKey;
        if (sourceKey.Length == 0)
            sourceKey = "local:" + product.Id;
        configuration.Products[product.Id] = details with
        {
            SourceKey = sourceKey
        };
        ShopSettingsStore.Save(database, configuration);
        transaction.Commit();
        return product.Id;
    }
}
