# shoping67

Windows Forms teaching shop (.NET 10), with an original shoping67 wordmark, home offers, a technical catalog, ET/RU/EN, basket and demo checkout. No real payments or deliveries.

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

The shop opens on Home with currently published promotions, three category links, and catalog products. Promotions have start/end dates, translated titles/descriptions, an optional banner, category/product targeting, discount percentage, and display position. Expired or unpublished offers are hidden. A 30-second timer updates the open storefront at promotion boundaries; checkout always validates current price, offer validity, visibility and stock.

Categories:

- Phones: brand, RAM, storage, screen, OS, 5G.
- Laptops / PCs: device type, brand, exact GPU, CPU, RAM, storage, screen, OS.
- Computer accessories: mice, keyboards, headphones; type, brand, connection, layout, switches and microphone.

Filters use OR between selected values of one characteristic and AND across characteristics. `RTX 4070`, `RTX 4070 SUPER` and `RTX 4070 Laptop GPU` are distinct values. Search also checks specification values. Price limits, stock filter, sorting and reset are supported. Missing source specifications are displayed as “Not specified” and never guessed.

Product details show translated text, specifications, current/old price, zoomable photo, quantity, Add to basket and Buy now. The basket survives language changes. Changing a price or an expiring promotion requires removing an outdated basket line and adding it again; stale discounted prices are rejected during checkout. The greatest matching discount applies; discounts do not stack or compound on refresh. In database mode completed demo orders reduce stock and appear in the existing sales report.

## Product sources and honest demo data

At shop opening the application requests the public DummyJSON smartphone/laptop endpoints and this repository's versioned `Tooded_2/Catalog/tech-products.json`. Network failures use cached and bundled data. Product images are requested separately and cached as PNG, including conversion of WebP; unavailable images show a placeholder.

DummyJSON is a **teaching API, not a supplier feed**. Its price numbers and stock are used only as illustrative shop values, not current offers or exchange rates. The source does not publish verified GPU/CPU/RAM specifications, so those fields are left empty. ET/RU use a translated import description; the original source description is available in EN. An administrator can write all three descriptions.

The bundled catalog contains 14 clearly labelled **reference configurations** with three-language descriptions and useful filter examples, including a desktop RTX 4070. They are sample configurations/prices, not claimed live retailer offers. Reference PCs without a verified photograph use an image placeholder. Administrators can supply a matching photograph and edit the specifications. The remote reference catalog is the same openly reviewable JSON file in this repository; it is not represented as an independent supplier.

Stable source keys prevent duplicate import. Existing admin edits, deleted imported products and sold stock are not reset by reimport. New source keys can be imported into a nonempty database. Existing unrelated legacy products remain in the administrator list; assign them a supported category, type and visibility in the new editor to show them in this storefront.

## Administration

Sign in as administrator and click **Promotions and specifications** (ET: `Kampaaniad ja omadused`, RU: `Акции и характеристики`).

- Products tab: select a row and Edit, or double-click. Edit ET/RU/EN names/descriptions, category, device type, brand, ordinary price, stock, visibility, image URL/file, and specification values. Empty values mean unknown. An incompatible device type/category cannot be saved.
- Promotions tab: Add, Edit or Delete. Set three titles, descriptions, optional HTTPS banner URL, discount 0–90%, start/end in the PC's local timezone, category, optional product selection, publication and display order. An empty product selection targets the whole chosen category; 0% is a featured collection.
- Reopen/refresh the storefront after saving. Product metadata and promotions are stored in SQL Server `ShopSettings`, independently of the internet cache. User language preference and downloaded files are stored under `%LOCALAPPDATA%\shoping67`.

`--demo` uses its own in-memory inventory and introductory offers, without administrative database access; it never modifies SQL Server. Closing and reopening demo mode resets its stock. The normal SQL-backed mode is required to edit and persist promotions as administrator. A school PC reset can remove its local SQL data and cache; Git preserves source code, not the database. Persistent shared data requires a permanent SQL Server or database backup.

## Languages

ET / RU / EN is available in the storefront, login and existing admin window. The choice is saved. Store navigation, filters, product descriptions, promotion text, basket, checkout, confirmation, receipt, registration and new management editors use the selected language. Model names and exact technical values remain unchanged. Unknown third-party descriptions are not silently presented as human translations.

## Validation

```powershell
dotnet restore Tooded_2.slnx
dotnet build Tooded_2.slnx --no-restore
dotnet run --project Tests/Shop.CoreChecks
```

`.github/workflows/shoping67-checks.yml` runs the same Windows build and core checks. Checks cover basket and atomic stock rules, exact GPU matching, combined filters, date boundaries, targeted/overlapping discounts, no compounding, expired-price rejection, settings roundtrip, missing API specs and three-language state preservation. These checks use local fixtures or fake HTTP responses, not external availability.

Windows manual checks: open demo, switch languages with a populated basket, filter desktop RTX 4070 + 32 GB, check no-result/reset flows, open/zoom a product, buy at the displayed discount. In normal mode, edit and reopen a promotion, schedule expiry, hide a product before checkout, reopen the application to check persistence, and check repeat import does not reset stock or duplicate products.
