using System.Globalization;

namespace Tooded_2.Shop;

/// <summary>Receipts/retries retain the language chosen at checkout, even after UI language changes.</summary>
public sealed class DocumentCulture : IDisposable
{
    private readonly CultureInfo culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo uiCulture = CultureInfo.CurrentUICulture;
    public DocumentCulture(string language)
    {
        var selected = CultureInfo.GetCultureInfo(language.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en-US" : "et-EE");
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = selected;
    }
    public void Dispose()
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }
}
