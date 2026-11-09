using System.Globalization;
using System.Resources;

namespace Tooded_2.Shop;

/// <summary>Runtime strings are read only from .resx resources; Estonian is the neutral language.</summary>
public static class UiText
{
    public static readonly ResourceManager Resources = new("Tooded_2.Resources.UiText", typeof(UiText).Assembly);
    public static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture)
        ?? throw new MissingManifestResourceException("Missing UI resource: " + key);
    public static string Format(string key, params object?[] values) => string.Format(CultureInfo.CurrentCulture, Get(key), values);
    public static string Category(string key) => key switch
    {
        "phones" => Get("Phones"),
        "computers" => Get("LaptopsPCs"),
        "accessories" => Get("ComputerAccessories"),
        _ => Get("AllProducts")
    };
    public static string Spec(string key) => key switch
    {
        "brand" => Get("Brand"), "kind" => Get("DeviceType"),
        "gpu" => Get("GraphicsCard"), "cpu" => Get("Processor"),
        "ram" => Get("RAM"), "storage" => Get("Storage"),
        "screen" => Get("Screen"), "os" => Get("OperatingSystem"),
        "5g" => "5G", "connection" => Get("Connection"),
        "layout" => Get("KeyboardLayout"),
        "switches" => Get("Switches"), "microphone" => Get("Microphone"),
        "sensor" => Get("Sensor"), "buttons" => Get("Buttons"),
        "drivers" => Get("Drivers"), "productCode" => Get("ProductCode"), _ => key
    };
    public static string Value(string value) => value switch
    {
        "phone" => Get("Phone"),
        "laptop" => Get("Laptop"), "desktop" => Get("DesktopPC"),
        "mouse" => Get("Mouse"), "keyboard" => Get("Keyboard"),
        "headphones" => Get("Headphones"),
        "wired" => Get("Wired"), "wireless" => Get("Wireless"),
        "Tactile" => Get("Tactile"),
        "yes" => Get("Yes"), "no" => Get("No"), _ => value
    };
    public static string Money(decimal value) => value.ToString("0.00", CultureInfo.CurrentCulture) + " €";
}
