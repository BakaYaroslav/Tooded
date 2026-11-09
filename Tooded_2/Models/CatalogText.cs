using System.Globalization;

namespace Tooded_2;
// Переводы содержимого каталога. Тексты кнопок и сообщений находятся отдельно в .resx.
public sealed record CatalogText(string Et = "", string En = "", string Ru = "")
{
    public string Value => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
    {
        "en" when !string.IsNullOrWhiteSpace(En) => En,
        "ru" when !string.IsNullOrWhiteSpace(Ru) => Ru,
        _ => Et
    };
}
