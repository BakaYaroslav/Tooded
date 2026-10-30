# shoping67

Windows Forms teaching shop (.NET 10), with an original shoping67 wordmark, home offers, a technical catalog, Estonian-only interface, basket and demo checkout. No real payments or deliveries.

## Windows startup

Open `Tooded_2.slnx` (or `Tooded_2/Tooded_2.csproj`) in Visual Studio with .NET 10 Windows Desktop support. For a shop without SQL Server or login:

```powershell
dotnet run --project Tooded_2 -- --demo
```

Normal startup retains login and the existing administrator. SQL Server Express LocalDB `(localdb)\MSSQLLocalDB` is required. The application applies EF migrations automatically, including the new `ShopSettings` table, without deleting existing products or users.

```powershell
dotnet run --project Tooded_2
```

## Home, catalog and filters

The shop opens on Home with currently published promotions, three category links, and catalog products. Promotions have start/end dates, Estonian titles/descriptions, an optional banner, category/product targeting, discount percentage, and display position. Expired or unpublished offers are hidden. A 30-second timer updates the open storefront at promotion boundaries; checkout always validates current price, offer validity, visibility and stock.

Categories:

- Phones: brand, RAM, storage, screen, OS, 5G.
- Laptops / PCs: device type, brand, exact GPU, CPU, RAM, storage, screen, OS.
- Computer accessories: mice, keyboards, headphones; type, brand, connection, layout, switches and microphone.

Filters use OR between selected values of one characteristic and AND across characteristics. `RTX 4070`, `RTX 4070 SUPER` and `RTX 4070 Laptop GPU` are distinct values. Search also checks specification values. Price limits, stock filter, sorting and reset are supported. Missing source specifications are displayed as “Not specified” and never guessed.

Product details show Estonian text, specifications, current/old price, zoomable photo, quantity, Add to basket and Buy now. The basket is retained while browsing. Changing a price or an expiring promotion requires removing an outdated basket line and adding it again; stale discounted prices are rejected during checkout. The greatest matching discount applies; discounts do not stack or compound on refresh. In database mode completed demo orders reduce stock and appear in the existing sales report.

## Product sources and photographs

The catalog contains real model names from Apple, ASUS, Dell, Google, Huawei, Lenovo, Logitech, Oppo, Realme, Samsung, Vivo and Beats. shoping67 is the shop name, never a hardware manufacturer. The invented shop-branded PCs/laptops and old reference configurations have been retired. The real ASUS ROG Strix G16 G614JI-AS94 provides the documented RTX 4070 Laptop GPU filter example.

At opening, the application updates its approved model list from the versioned catalog and the DummyJSON phone, laptop and selected headphone feeds. Every incoming entry must match the approved source ID, model name, brand, category and device type. Unknown IDs, cancelled/out-of-category accessories and mismatched items are rejected. Stored cache records pass the same validation. Prices/stock remain illustrative teaching-shop values, not a live supplier feed or currency conversion.

Every catalog model has a matching photograph bundled under `Tooded_2/Images/Products`. The catalog records its photo URL and product/source URL. The Windows build workflow downloads and decodes the photographs, tests every PNG and saves the verified image files with the project. Storefront photos then load locally even without internet. Missing source specifications remain unknown; no GPU/RAM is inferred from an unrelated model.

On the first SQL-backed shop opening after updating, catalog reconciliation archives the old invented products and incorrectly categorized records, repairs approved image paths and imports the real models. It preserves database rows, users and sales history. Reconciliation is idempotent and does not reset stock or prices of valid existing models. An incorrect imported source identity is released for a correct reimport. No database deletion is needed.

## Administration

Sign in as administrator and click **Promotions and specifications** (ET: `Kampaaniad ja omadused`, RU: `Акции и характеристики`).

- Products tab: select a row and Edit, or double-click. Edit Estonian names/descriptions, category, device type, brand, ordinary price, stock, visibility, image URL/file, and specification values. Empty values mean unknown. An incompatible device type/category cannot be saved.
- Promotions tab: Add, Edit or Delete. Set the Estonian title/description, optional HTTPS banner URL, discount 0–90%, start/end in the PC's local timezone, category, optional product selection, publication and display order. Enable **Entire selected category** to target all its visible products, or disable it and select individual products. The list shows only visible products in that category. Hidden/retired items and incompatible category selections cannot be saved; the editor shows the target count. Existing invalid offers must be corrected by choosing the category and current products again. 0% is a featured collection.
- Reopen/refresh the storefront after saving. Product metadata and promotions are stored in SQL Server `ShopSettings`, independently of the internet cache. User language preference and downloaded files are stored under `%LOCALAPPDATA%\shoping67`.

`--demo` uses its own in-memory inventory and introductory offers, without administrative database access; it never modifies SQL Server. Closing and reopening demo mode resets its stock. The normal SQL-backed mode is required to edit and persist promotions as administrator. A school PC reset can remove its local SQL data and cache; Git preserves source code, not the database. Persistent shared data requires a permanent SQL Server or database backup.

## Language

The current interface is Estonian only. Language selectors have been removed from the storefront, login and administrator window. Old RU/EN preferences are ignored and prices/dates use `et-EE`. Product and promotion editors require only the Estonian name/title and description; existing foreign-language fields remain stored for compatibility but are neither shown nor required. Model names and exact technical values are preserved.

## Validation

```powershell
dotnet restore Tooded_2.slnx
dotnet build Tooded_2.slnx --no-restore
dotnet run --project Tests/Shop.CoreChecks
```

`.github/workflows/shoping67-checks.yml` runs the same Windows build and core checks. Checks cover basket and atomic stock rules, exact GPU matching, combined filters, date boundaries, targeted/overlapping discounts, no compounding, expired-price rejection, settings roundtrip, missing API specs and Estonian-only behavior and ignored legacy language preferences. These checks use local fixtures or fake HTTP responses, not external availability.

Windows manual checks: open demo, check the Estonian-only storefront and editors, filter ASUS RTX 4070 Laptop GPU + 16 GB, check no-result/reset flows, open/zoom a product, buy at the displayed discount. In normal mode, edit and reopen a promotion, schedule expiry, hide a product before checkout, reopen the application to check persistence, and check repeat import does not reset stock or duplicate products.
