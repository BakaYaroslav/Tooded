using System.Globalization;
using System.Text.Json;

namespace Tooded_2.Shop;

public static class T
{
    private static readonly string preference = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "shoping67", "language.txt");
    public static string Language { get; private set; } = ReadLanguage();
    private static string ReadLanguage()
    {
        try { string value = File.ReadAllText(preference).Trim(); return new[] { "et", "ru", "en" }.Contains(value) ? value : "et"; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return "et"; }
    }
    public static void SetLanguage(string value)
    {
        Language = new[] { "et", "ru", "en" }.Contains(value) ? value : "et";
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(Language == "et" ? "et-EE" : Language == "ru" ? "ru-RU" : "en-GB");
        try { Directory.CreateDirectory(Path.GetDirectoryName(preference)!); File.WriteAllText(preference, Language); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
    public static string L(string et, string ru, string en) => Language == "ru" ? ru : Language == "en" ? en : et;
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
    private static readonly Dictionary<string, string[]> legacy = new(StringComparer.Ordinal)
    {
        ["Sisselogimine"] = new[] { "Вход", "Sign in" },
        ["Kasutajanimi:"] = new[] { "Имя пользователя:", "Username:" },
        ["Parool:"] = new[] { "Пароль:", "Password:" },
        ["Logi sisse"] = new[] { "Войти", "Sign in" },
        ["Registreeri"] = new[] { "Зарегистрироваться", "Register" },
        ["Registreerimine"] = new[] { "Регистрация", "Registration" },
        ["Korda parooli:"] = new[] { "Повтор пароля:", "Repeat password:" },
        ["Tühista"] = new[] { "Отмена", "Cancel" },
        ["Toode nimetus: "] = new[] { "Название товара: ", "Product name: " },
        ["Kogus: "] = new[] { "Количество: ", "Quantity: " },
        ["Hind: "] = new[] { "Цена: ", "Price: " },
        ["Kategooriad: "] = new[] { "Категории: ", "Categories: " },
        ["Lisa kategooriad"] = new[] { "Добавить категорию", "Add category" },
        ["Kustuta kategooriad"] = new[] { "Удалить категорию", "Delete category" },
        ["Otsi pilt"] = new[] { "Выбрать фото", "Choose image" },
        ["Lisa"] = new[] { "Добавить", "Add" },
        ["Kustuta"] = new[] { "Удалить", "Delete" },
        ["Uuenda"] = new[] { "Обновить", "Refresh" },
        ["Puhasta"] = new[] { "Очистить", "Clear" },
        ["Pood"] = new[] { "Магазин", "Shop" },
        ["Müüdud: 0 tk"] = new[] { "Продано: 0 шт.", "Sold: 0" },
        ["Admin Panel"] = new[] { "Панель администратора", "Admin panel" },
        ["Sisesta kasutajanimi ja parool!"] = new[] { "Введите имя пользователя и пароль!", "Enter a username and password!" },
        ["Vale kasutajanimi või parool!"] = new[] { "Неверное имя пользователя или пароль!", "Incorrect username or password!" },
        ["Sisesta toote nimetus!"] = new[] { "Введите название товара!", "Enter a product name!" },
        ["Kogus peab olema täisarv!"] = new[] { "Количество должно быть целым числом!", "Quantity must be an integer!" },
        ["Hind peab olema arv!"] = new[] { "Цена должна быть числом!", "Price must be a number!" },
        ["Vali kategooria!"] = new[] { "Выберите категорию!", "Select a category!" },
        ["Vali tabelist toode!"] = new[] { "Выберите товар в таблице!", "Select a product in the table!" },
        ["Toodet ei leitud!"] = new[] { "Товар не найден!", "Product not found!" },
        ["Kas kustutada toode?"] = new[] { "Удалить товар?", "Delete this product?" },
        ["Kinnitus"] = new[] { "Подтверждение", "Confirmation" },
        ["Kirjuta kategooria nimi kategooria lahtrisse!"] = new[] { "Введите название категории!", "Enter a category name!" },
        ["Selline kategooria on juba olemas!"] = new[] { "Такая категория уже существует!", "This category already exists!" },
        ["Kategooriat ei saa kustutada, selles on tooteid!"] = new[] { "Нельзя удалить категорию с товарами!", "A category containing products cannot be deleted!" },
        ["Viga: "] = new[] { "Ошибка: ", "Error: " },
        ["Ostukorv"] = new[] { "Корзина", "Basket" },
        ["Sinu ostukorv"] = new[] { "Ваша корзина", "Your basket" },
        ["Vormista tellimus"] = new[] { "Оформить заказ", "Checkout" },
        ["Tühjenda korv"] = new[] { "Очистить корзину", "Empty basket" },
        ["Jätka ostlemist"] = new[] { "Продолжить покупки", "Continue shopping" },
        ["Ostukorv on tühi. Leia kataloogist midagi meelepärast."] = new[] { "Корзина пуста. Выберите товары в каталоге.", "Your basket is empty. Explore the catalog." },
        ["Eemalda"] = new[] { "Удалить", "Remove" },
        ["Lisa ostukorvi"] = new[] { "В корзину", "Add to basket" },
        ["Osta kohe"] = new[] { "Купить сейчас", "Buy now" },
        ["Hetkel otsas"] = new[] { "Нет в наличии", "Out of stock" },
        ["Otsas"] = new[] { "Нет в наличии", "Out of stock" },
        ["Kõik on korvis"] = new[] { "Всё уже в корзине", "All in basket" },
        ["Kõik on ostukorvis"] = new[] { "Всё уже в корзине", "All in basket" },
        ["Lisatud ostukorvi"] = new[] { "Добавлено в корзину", "Added to basket" },
        ["Klõpsa pildil, et suurendada"] = new[] { "Нажмите на фото для увеличения", "Click the image to enlarge" },
        ["Toote kirjeldus"] = new[] { "Описание товара", "Description" },
        ["Toote andmed"] = new[] { "Характеристики", "Specifications" },
        ["Kogus"] = new[] { "Количество", "Quantity" },
        ["Tootekood"] = new[] { "Код товара", "Product code" },
        ["Ainult demomakse. Päris raha ei võeta, kaardiandmeid ei küsita ja kaupu ei saadeta."] = new[] { "Учебная оплата. Деньги не списываются, данные карты не нужны, товары не отправляются.", "Demo checkout. No money is charged, no card details are requested and no goods are shipped." },
        ["Tellimuse vormistamine · Demomakse"] = new[] { "Оформление заказа · Учебная оплата", "Checkout · Demo payment" },
        ["Vaata tellimus üle"] = new[] { "Проверьте заказ", "Review your order" },
        ["Kinnitan, et tegemist on demotellimusega"] = new[] { "Подтверждаю, что это учебный заказ", "I confirm that this is a demo order" },
        ["Kinnita demomakse"] = new[] { "Подтвердить учебную оплату", "Confirm demo payment" },
        ["Tagasi korvi"] = new[] { "Назад в корзину", "Back to basket" },
        ["Tellimust ei kinnitatud. Kontrolli laoseisu ja hindu, uuenda kataloogi ning proovi uuesti. Ostukorv on alles."] = new[] { "Заказ не подтверждён. Проверьте цены, акции и наличие, обновите каталог и попробуйте ещё раз. Корзина сохранена.", "Order not confirmed. Check prices, promotions and stock, refresh the catalog and try again. Your basket is preserved." },
        ["Tellimus ei õnnestunud"] = new[] { "Не удалось оформить заказ", "Order failed" },
        ["Tellimus kinnitatud"] = new[] { "Заказ подтверждён", "Order confirmed" },
        ["Aitäh! Demomakse õnnestus."] = new[] { "Спасибо! Учебная оплата прошла.", "Thank you! Demo payment completed." },
        ["Raha ei võetud. Tellimuse kinnitus on allpool."] = new[] { "Деньги не списаны. Подтверждение заказа ниже.", "No money was charged. Your order confirmation is below." },
        ["Salvesta kinnitus"] = new[] { "Сохранить подтверждение", "Save confirmation" },
        ["Faili ei saanud salvestada. Vali teine asukoht."] = new[] { "Не удалось сохранить файл. Выберите другую папку.", "Unable to save the file. Choose another location." },
        ["Salvestamine"] = new[] { "Сохранение", "Save" },
        ["Vali vähemalt üks toode."] = new[] { "Выберите хотя бы один товар.", "Select at least one item." },
        ["Toote hind ei saa olla negatiivne."] = new[] { "Цена товара не может быть отрицательной.", "A product price cannot be negative." },
        ["Ostukorv on tühi."] = new[] { "Корзина пуста.", "The basket is empty." },
        ["Ostukorvis on korduvad tooteread."] = new[] { "В корзине повторяющиеся позиции.", "The basket has duplicate product rows." },
        ["Pilt puudub"] = new[] { "Нет изображения", "No image" },
        ["Salvesta"] = new[] { "Сохранить", "Save" },
        ["Kasutajanimi peab olema vähemalt 3 märki!"] = new[] { "Имя пользователя должно содержать хотя бы 3 символа!", "Username must contain at least 3 characters!" },
        ["Parool peab olema vähemalt 4 märki!"] = new[] { "Пароль должен содержать хотя бы 4 символа!", "Password must contain at least 4 characters!" },
        ["Paroolid ei ühti!"] = new[] { "Пароли не совпадают!", "Passwords do not match!" },
        ["Selline kasutajanimi on juba olemas!"] = new[] { "Такое имя пользователя уже существует!", "This username already exists!" },
        ["Registreerimine õnnestus! Nüüd saad sisse logida."] = new[] { "Регистрация завершена. Теперь можно войти.", "Registration complete. You can now sign in." },
        ["Kampaaniad ja omadused"] = new[] { "Акции и характеристики", "Promotions and specifications" },
        ["Müüdud: {0} tk | Tulu kokku: {1}"] = new[] { "Продано: {0} шт. | Выручка: {1}", "Sold: {0} | Revenue: {1}" },
        ["Otsi toodet, mudelit või omadust…"] = new[] { "Название, модель или характеристика…", "Search products, models or specifications…" },
        ["Otsing"] = new[] { "Поиск", "Search" },
        ["Avaleht ja pakkumised"] = new[] { "Главная и акции", "Home and offers" },
        ["Soovitatud"] = new[] { "Рекомендуемые", "Recommended" },
        ["Hind: odavamad enne"] = new[] { "Цена по возрастанию", "Price: low to high" },
        ["Hind: kallimad enne"] = new[] { "Цена по убыванию", "Price: high to low" },
        ["Nimi: A–Z"] = new[] { "Название: А–Я", "Name: A–Z" },
        ["Laadin veebikataloogi…"] = new[] { "Загружаю интернет-каталог…", "Loading online catalog…" },
        ["Kataloogi ei saanud laadida. "] = new[] { "Не удалось загрузить каталог. ", "Unable to load catalog. " },
        ["Õppepood · Demohinnad ja demomakse. Konfiguratsiooninäited on märgistatud."] = new[] { "Учебный магазин · Демоцены и учебная оплата. Примеры конфигураций отмечены.", "Teaching shop · Demo prices and payment. Reference configurations are labelled." },
        ["Filtrid"] = new[] { "Фильтры", "Filters" },
        ["Hind (€): alates / kuni"] = new[] { "Цена (€): от / до", "Price (€): from / to" },
        ["Ainult laos"] = new[] { "Только в наличии", "In stock only" },
        ["Puhasta filtrid"] = new[] { "Сбросить фильтры", "Clear filters" },
        ["Värsked pakkumised"] = new[] { "Предложения и акции", "Latest offers" },
        ["Aktiivseid kampaaniaid praegu pole. Vaata kataloogi."] = new[] { "Сейчас нет действующих акций. Посмотрите каталог.", "No promotions are active. Explore the catalog." },
        ["Avasta kolm kategooriat"] = new[] { "Выбери свою категорию", "Explore our three categories" },
        ["Valik kataloogist"] = new[] { "Товары из каталога", "From the catalog" },
        ["Laadimine…"] = new[] { "Загрузка…", "Loading…" },
        ["Sobivaid tooteid ei leitud. Muuda filtreid."] = new[] { "Подходящих товаров нет. Измените фильтры.", "No matching products. Adjust your filters." },
        ["Kehtib kuni "] = new[] { "Действует до ", "Ends " },
        ["Vaata pakkumist →"] = new[] { "Посмотреть предложение →", "Explore offer →" },
        ["tk"] = new[] { "шт.", "item" },
        ["Kokku ({0} tk): {1}"] = new[] { "Итого ({0} шт.): {1}", "Total ({0} items): {1}" },
        ["Veebikataloog uuendatud. Hinnad ja tellimused on õppeotstarbelised."] = new[] { "Интернет-каталог обновлён. Цены и заказы учебные.", "Online catalog updated. Prices and orders are for teaching." },
        ["Võrk pole saadaval. Kasutusel salvestatud ja kaasasolev kataloog."] = new[] { "Сеть недоступна. Используется сохранённый и встроенный каталог.", "Network unavailable. Using cached and bundled products." },
        ["Toodet „{0}” on laos {1} tk."] = new[] { "Товар «{0}»: в наличии {1} шт.", "Product {0}: {1} in stock." },
        ["shoping67 · NÄIDISTELLIMUS"] = new[] { "shoping67 · УЧЕБНЫЙ ЗАКАЗ", "shoping67 · DEMO ORDER" },
        ["Tellimus"] = new[] { "Заказ", "Order" },
        ["Kuupäev"] = new[] { "Дата", "Date" },
        ["TASUTUD DEMOMAKSEGA · Raha ei võetud."] = new[] { "УЧЕБНАЯ ОПЛАТА · Деньги не списаны.", "DEMO PAYMENT · No money was charged." },
        ["KOKKU"] = new[] { "ИТОГО", "TOTAL" },
        ["Tegemist on õppemängu tellimusega. Kaupu ei saadeta."] = new[] { "Это учебный заказ. Товары не отправляются.", "This is a demo order. No goods are shipped." },
        ["Laoseis muutus. Uuenda kataloogi ja ostukorvi."] = new[] { "Наличие изменилось. Обновите каталог и корзину.", "Stock changed. Refresh the catalog and basket." },
        ["Hind või soodustus muutus. Lisa toode uuesti korvi."] = new[] { "Цена или скидка изменилась. Добавьте товар в корзину заново.", "The price or promotion changed. Add the product to your basket again." },
        ["Sisesta pealkiri kolmes keeles."] = new[] { "Введи заголовок на трёх языках.", "Enter a title in all three languages." },
        ["Kontrolli kuupäevi, kategooriat ja allahindlust (0–90%)."] = new[] { "Проверь даты, категорию и скидку (0–90%).", "Check dates, category and discount (0–90%)." },
        ["Näidiskonfiguratsioon · demohind"] = new[] { "Пример конфигурации · демоцена", "Reference configuration · demo price" },
        ["Märkimata"] = new[] { "Не указано", "Not specified" },
        ["Saadaval {0} tk"] = new[] { "В наличии {0} шт.", "{0} available" },
        ["Haldus"] = new[] { "Управление", "Management" },
        ["Tooted ja omadused"] = new[] { "Товары и характеристики", "Products and specifications" },
        ["Pakkumised ja kampaaniad"] = new[] { "Предложения и акции", "Offers and promotions" },
        ["Muuda valitud toodet"] = new[] { "Изменить выбранный товар", "Edit selected product" },
        ["Lisa kampaania"] = new[] { "Добавить акцию", "Add promotion" },
        ["Nimetus"] = new[] { "Название", "Name" },
        ["Kategooria"] = new[] { "Категория", "Category" },
        ["Hind"] = new[] { "Цена", "Price" },
        ["Laoseis"] = new[] { "Остаток", "Stock" },
        ["Nähtav"] = new[] { "Показывать", "Visible" },
        ["Pealkiri"] = new[] { "Заголовок", "Title" },
        ["Allahindlus %"] = new[] { "Скидка %", "Discount %" },
        ["Algus"] = new[] { "Начало", "Starts" },
        ["Lõpp"] = new[] { "Окончание", "Ends" },
        ["Aktiivne"] = new[] { "Действует", "Active" },
        ["Järjekord"] = new[] { "Порядок", "Position" },
        ["Kustuta kampaania?"] = new[] { "Удалить акцию?", "Delete this promotion?" },
        ["Kampaania"] = new[] { "Акция", "Promotion" },
        ["Kirjeldus"] = new[] { "Описание", "Description" },
        ["Allahindlus % (0 = ainult pakkumine)"] = new[] { "Скидка % (0 = подборка без скидки)", "Discount % (0 = collection without discount)" },
        ["Algus / lõpp (kohalik aeg)"] = new[] { "Начало / окончание (местное время)", "Start / end (local time)" },
        ["Avaldatud"] = new[] { "Опубликовано", "Published" },
        ["Bänneri pildi HTTPS-aadress (valikuline)"] = new[] { "HTTPS-адрес баннера (необязательно)", "Banner image HTTPS URL (optional)" },
        ["Valitud tooted (tühi = kogu kategooria)"] = new[] { "Товары (ничего не выбрано = вся категория)", "Products (none selected = entire category)" },
        ["Kasuta HTTPS-aadressi."] = new[] { "Укажите HTTPS-адрес.", "Use an HTTPS URL." },
        ["Tavahind (€)"] = new[] { "Обычная цена (€)", "Regular price (€)" },
        ["Näita poes"] = new[] { "Показывать в магазине", "Show in shop" },
        ["Pildi HTTPS-aadress või kohalik fail"] = new[] { "HTTPS-адрес фото или локальный файл", "Image HTTPS URL or local file" },
        ["Omadused · tühi väärtus = teadmata"] = new[] { "Характеристики · пустое значение = неизвестно", "Specifications · empty value = unknown" },
        ["Omadus"] = new[] { "Характеристика", "Specification" },
        ["Väärtus"] = new[] { "Значение", "Value" },
        ["Sisesta nimi kolmes keeles."] = new[] { "Введите название на трёх языках.", "Enter a name in all three languages." },
        ["Omadus puudub või kordub."] = new[] { "Характеристика не выбрана или повторяется.", "A specification is missing or duplicated." },
        ["Seadme tüüp ei sobi kategooriaga."] = new[] { "Тип устройства не соответствует категории.", "The device type does not match the category." },
        ["Makseviis: demomakse. Raha ei võeta ja kaupu ei saadeta."] = new[] { "Учебная оплата: деньги не списываются, товары не отправляются.", "Demo payment: no money is charged and no goods are shipped." },
        ["Laoseis muutub selle seansi jooksul."] = new[] { "Остатки меняются в текущем сеансе.", "Stock changes last for this session." },
        ["Tellimus salvestatakse müügiaruandesse."] = new[] { "Заказ сохраняется в отчёте о продажах.", "The order is saved to the sales report." },
        [" · Näidiskonfiguratsioon"] = new[] { " · Пример конфигурации", " · Reference configuration" },
        ["Laos {0} tk · Korvis {1}"] = new[] { "В наличии {0} шт. · В корзине {1}", "In stock: {0} · In basket: {1}" },
        ["Laoseis või hind muutus."] = new[] { "Наличие или цена изменились.", "Stock or price changed." },
    };
    public static string Legacy(string value)
    {
        if (legacy.TryGetValue(value, out var pair)) return L(value, pair[0], pair[1]);
        return value;
    }
    public static string Canonical(string value)
    {
        if (legacy.ContainsKey(value)) return value;
        return legacy.FirstOrDefault(p => p.Value.Contains(value)).Key ?? value;
    }
}
