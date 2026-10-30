using System.Globalization;

namespace Tooded_2.Shop;

public static class T
{
    public static string Language => "et";
    public static void SetLanguage(string value)
    {
        // Ignore language preferences saved by earlier versions.
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("et-EE");
    }
    public static string L(string et, string ru, string en) => et;
    public static string Category(string key) => key switch
    {
        "phones" => L("Telefonid", "Телефоны", "Phones"),
        "computers" => L("Sülearvutid / lauaarvutid", "Ноутбуки / ПК", "Laptops / PCs"),
        "accessories" => L("Arvutitarvikud", "Компьютерные аксессуары", "Computer accessories"),
        _ => L("Kõik tooted", "Все товары", "All products")
    };
    public static string Spec(string key) => key switch
    {
        "brand" => L("Kaubamärk", "Бренд", "Brand"), "kind" => L("Seadme tüüp", "Тип устройства", "Device type"),
        "gpu" => L("Videokaart", "Видеокарта", "Graphics card"), "cpu" => L("Protsessor", "Процессор", "Processor"),
        "ram" => L("Muutmälu", "Оперативная память", "RAM"), "storage" => L("Salvestusruum", "Накопитель", "Storage"),
        "screen" => L("Ekraan", "Экран", "Screen"), "os" => L("Operatsioonisüsteem", "Операционная система", "Operating system"),
        "5g" => "5G", "connection" => L("Ühendus", "Подключение", "Connection"),
        "layout" => L("Klaviatuuripaigutus", "Раскладка", "Keyboard layout"),
        "switches" => L("Lülitid", "Переключатели", "Switches"), "microphone" => L("Mikrofon", "Микрофон", "Microphone"),
        "sensor" => L("Andur", "Сенсор", "Sensor"), "buttons" => L("Nuppude arv", "Количество кнопок", "Buttons"),
        "drivers" => L("Kõlarielement", "Динамики", "Drivers"), _ => key
    };
    public static string Value(string value) => value switch
    {
        "phone" => L("Telefon", "Телефон", "Phone"),
        "laptop" => L("Sülearvuti", "Ноутбук", "Laptop"), "desktop" => L("Lauaarvuti", "ПК", "Desktop PC"),
        "mouse" => L("Hiir", "Мышь", "Mouse"), "keyboard" => L("Klaviatuur", "Клавиатура", "Keyboard"),
        "headphones" => L("Kõrvaklapid", "Наушники", "Headphones"),
        "wired" => L("Juhtmega", "Проводное", "Wired"), "wireless" => L("Juhtmevaba", "Беспроводное", "Wireless"),
        "Tactile" => L("Taktiilsed", "Тактильные", "Tactile"),
        "yes" => L("Jah", "Да", "Yes"), "no" => L("Ei", "Нет", "No"), _ => value
    };
    public static string Money(decimal value) => value.ToString("0.00", CultureInfo.CurrentCulture) + " €";
    public static string Legacy(string value) => value;
    public static string Canonical(string value) => value;
}
