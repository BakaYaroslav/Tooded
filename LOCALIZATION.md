# WinForms mitmekeelsus resx-failidega

Rakenduses on ülesande järgi kaks keelt: **Eesti** (`et-EE`, vaikimisi) ja **English** (`en-US`).

## Ülesande ja koodi seos

| Ülesande osa | Teostus |
| --- | --- |
| Vormi tõlgitavad omadused resx-failides | Näiteks `LoginForm.resx` ja `LoginForm.en.resx`; `InitializeComponent()` kasutab `ComponentResourceManager.ApplyResources`. Samad ressursipaarid on teistel vormidel. |
| Eesti on vaikekeel | Neutraalsed `.resx`-failid sisaldavad eestikeelseid tekste, `.en.resx` sisaldab ingliskeelseid tekste. `NeutralLanguage` on `et`. |
| `comboLanguage`: Eesti / English | `LanguageManager.CreateSelector()` loob nimega `comboLanguage` valiku sisselogimisaknas ja poe päises. |
| `_keelLaetud` kaitse | Valiku algväärtustamisel ja teise avatud valiku sünkroonimisel ei käivitata uut keelevahetust. |
| Kultuuri muutmine | `LanguageManager.ChangeLanguage()` määrab `Thread.CurrentThread.CurrentUICulture` ja `CurrentCulture`. Uued töötajad saavad sama vaikekultuuri. |
| Rekursiivne ressursside rakendamine | `ApplyResourcesToControl()` läbib vormi kontrollipuu; `ApplyResources(form, "$this")` uuendab pealkirja. |
| Keelevahetus ilma taaskäivitamiseta | `LocalizedForm` reageerib sündmusele ja rakendab ressursid olemasolevatele kontrollidele. Vormi ei looda uuesti. |
| Keele salvestamine | `Properties/Settings.settings`: `UserLanguage`, `String`, `User`, `et-EE`. Muutus salvestatakse `Properties.Settings.Default.Save()` abil. |
| Valitud keel järgmisel käivitamisel | `Program.Main()` kutsub `LanguageManager.Initialize()` enne `ApplicationConfiguration.Initialize()` ja vormide loomist. |

## Staatilised ja dünaamilised tekstid

Designeriga loodud vormide tekstid on vastava vormi ressurssides ning kontrollidel on püsiv `Name`. Visual Studios saab vormi avada Design-vaates ning valida `Language = English`, et muuta selle keele ressursse; `(Default)` tähendab eesti keelt. Tekstimuudatused tuleb hoida `.resx`-failides, mitte kirjutada tagasi `*.Designer.cs` kõvakodeeritud tekstidena.

Poekaardid, ostukorvi read, valideerimisteated ja tšekid luuakse jooksvalt. Nende sõnumid asuvad ühises paaris `Resources/UiText.resx` / `UiText.en.resx`, mida loeb .NET `ResourceManager`. `LanguageManager.Bind` jätab alles tekstiarvutuse, et näiteks kogused ja summad uuenevad uues keeles. See ei ole vana eestikeelse lähteteksti järgi tõlkimine: kasutatakse püsivaid ressursivõtmeid.

Ressurssidesse ei kirjutata kasutaja sisestatud `TextBox.Text`, valitud kategooria ID-d, laoseisu ega ostukorvi koguseid. Kategooriate ja omaduste tehnilised võtmed (`computers`, `gpu`, jne) jäävad mõlemas keeles samaks.

Toodete nimed ja administraatori kirjutatud kampaaniate sisu on andmed, mitte vormi ressursid. Olemasolevat ingliskeelset sisu võib kuvada; selle puudumisel säilib eestikeelne sisu. Vana vene UI ja `T.cs` on eemaldatud. Varasemad JSON-andmed jäävad loetavaks ilma andmebaasi kustutamata.

## Kontrollimine

1. Ava pood või sisselogimine; vali **English**. Tekstid muutuvad kohe.
2. Muuda otsingut, filtreid, ostukorvi ning sisesta e-post. Vaheta keelt mõlemas suunas: andmed säilivad.
3. Sule ja ava programm: viimane keel taastub.
4. Ava teine aken: see kasutab valitud keelt.
5. PDF ja e-kiri kasutavad ostu kinnitamise ajal valitud keelt; hilisem kordussaatmine ei muuda tellimust.

`Shop.CoreChecks` kontrollib mõlema ressurssikomplekti võtmeid ja kohatäiteid. `--ui-check` kontrollib avatud vormide keelevahetust, sisestatud andmete säilimist ning salvestatud keelt eraldi käivitatud protsessis. `Shop.StorageChecks` loeb eesti- ja ingliskeelseid PDF-tšekke sõltumatu PDF-lugejaga. Testid ei saada päris e-kirju.
