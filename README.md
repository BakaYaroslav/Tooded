# Tooded

.NET 10 Windows Forms õppepood: toodete haldus, kataloog, toote üksikasjad, ostukorv ja **ainult demomakse**. Kasutajaliides on eesti keeles.

## Käivitamine Windowsis

Vajalik on .NET 10 SDK koos Windows Desktopi toega.

```powershell
dotnet run --project Tooded_2 -- --demo
```

`--demo` avab poe otse ilma sisselogimise, SQL Serveri või LocalDB-ta. Esimesel avamisel laaditakse tooted, kirjeldused ja fotod automaatselt avalikust [Fake Store API-st](https://fakestoreapi.com/products). Kataloogi laadimise ajal jääb aken kasutatavaks. API hinnanumbrit kasutatakse eurodes näidishinnana, mitte valuutavahetusena, ning iga veebitoode saab õppepoe näidislaoseisu 20 tk. Need ei ole päris pakkumised ega tarnija laoseis. Demorežiimi laoseis säilib ainult avatud rakenduse jooksul; „Uuenda” ei taasta ostetud koguseid.

Allalaaditud kataloog ja fotod salvestatakse kausta `%LOCALAPPDATA%\\Tooded\\OnlineCatalog`. Kui internet ei tööta, kasutatakse varem laaditud kataloogi. Kui seda veel pole, näitab `--demo` kuut kaasasolevat toodet failist `Tooded_2/Catalog/products.json`. Akna allservas on näha kasutatav allikas. Uue internetipäringu tegemiseks demorežiimis ava rakendus uuesti.

Tavaline käivitamine säilitab olemasoleva sisselogimise ja administraatori vaate:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
dotnet ef database update --project Tooded_2
dotnet run --project Tooded_2
```

See töövoog vajab Windowsis SQL Server Express LocalDB instantsi `(localdb)\MSSQLLocalDB`. Andmebaas on `Tooded_2`. Kui `dotnet-ef` on juba paigaldatud, jäta tööriista paigaldamine vahele. Rakendus rakendab käivitamisel migraatsioonid ja loob esimese administraatori; muuda selle näidiskonto parool enne pärisandmetega kasutamist.

## Ostlemine

1. Otsi nime või kategooria järgi, vali kategooria, laofilter või hinna/nime sortimine.
2. Klõpsa tootel või selle pildil: avaneb kirjeldus, andmed, suurendatav foto ja koguse valik. „Lisa ostukorvi” lisab toote; „Osta kohe” lisab selle ja avab korvi.
3. Korvis saab koguseid muuta, ridu eemaldada ja korvi tühjendada. Sulgemine või makse tühistamine jätab korvi alles sama poeakna jooksul.
4. „Vormista tellimus” näitab kokkuvõtet. Kinnita demotellimuse märkeruut ning vajuta „Kinnita demomakse”. Kaardiandmeid ei küsita, raha ei võeta ja kaupa ei saadeta.
5. Eduka tellimuse järel saab kinnituse tekstifailina salvestada. Korv tühjeneb ja laoseis uueneb.

Andmebaasirežiimis salvestatakse demomüük olemasolevasse `Muugitabel` tabelisse ning laoseis väheneb. Need kirjed kajastuvad olemasolevas müügiaruandes: kasuta õppeandmebaasi. Hinnad ja laoseis kontrollitakse uuesti ostu hetkel. Kõik read salvestatakse ühe tehinguna; tingimuslikud uuendused takistavad sama laoseisu samaaegset ülemüüki. Ebaõnnestunud makse korral jääb ostukorv alles. Laoseisu või hinna muutumise järel uuenda kataloogi, eemalda aegunud rida korvist ja lisa uuesti.

Olemasolevatele andmebaasitoodetele saab lisada kirjeldused ja tehnilised andmed JSON-kataloogi kaudu: `name` peab täpselt vastama tootenimele (tõstutundetu). Nende toodete hind, laoseis ja kategooria tulevad endiselt andmebaasist. Kui andmebaasi tooted on tühjad, impordib poe avamine veebikataloogi automaatselt koos kategooriate, hindade, näidislaoseisu ja fotodega. Olemasolevaid tooteid ega müügiga vähendatud laoseisu ei kirjutata üle. Järgmised avamised kasutavad salvestatud tooteid; kirjeldused ja andmed loetakse allalaaditud kataloogi vahemälust. Võrguvea korral kasutatakse veebikataloogi vahemälu; kui ka seda pole, kuvatakse viga ja importi saab korrata nupuga „Uuenda”. Kaasasolevat kuue toote JSON-kataloogi ei impordita andmebaasi. Kui andmebaasis pilti pole määratud, kasutatakse vastava kataloogikirje pilti. Muul juhul kasutatakse administraatori määratud pilti. Vasteta toode kuvab üldinfo ega päri võõra toote tehnilisi andmeid.

Uus ostuvoog ei saada e-kirju ega kasuta SMTP-t. Piltide päritolu on kirjas failis `Tooded_2/Images/Catalog/SOURCES.md`.

## Kontrollid Linuxis või Windowsis

```sh
dotnet restore Tooded_2.slnx -p:EnableWindowsTargeting=true
dotnet build Tooded_2.slnx --no-restore -p:EnableWindowsTargeting=true
dotnet run --project Tests/Shop.CoreChecks
```

Sõltuvusteta kontrollprojekt testib korvi arvutusi, kogusepiiranguid, aegunud hindu/laoseisu, tellimuse terviklikkust, samaaegseid demooste, kinnitust ja pakitud kataloogi/pilte. Linuxis saab kompileerida ning käivitada põhiloogika kontrolle; Windows Formsi kasutajaliidese käivitamine ja LocalDB integratsioon vajavad Windowsi.

Windowsi käsikontroll: ava `--demo`, filtreeri/sordi, ava ka otsas toode, suurenda pilti, lisa sama toode mitu korda, muuda korvis kogust, eemalda rida, tühista vormistamine, seejärel kinnita demomakse ja salvesta kinnitus. Kontrolli korvi tühjenemist ning laoseisu vähenemist. Andmebaasirežiimis kontrolli lisaks müügiaruannet ja kahest poeaknast viimase toote ostmist.

Veebikataloogi kontrollid kasutavad asendatud HTTP-vastuseid: API väljade teisendus, vigase/tühja kataloogi tagasilükkamine, vahemälu salvestamine, võrguvea korral taastamine ning demo varukataloog. Windowsis kontrolli lisaks tühja andmebaasiga poe avamist, internetiühenduse katkemist ja seda, et teine avamine ei lisa korduvaid tooteid ega taasta müüdud laoseisu.
