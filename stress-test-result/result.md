# Stressz teszt jegyzőkönyv

A teszt célja a Google Cloud Run skálázódási képességének mérése volt. A terhelést egy Locust szkript biztosította, ami szintén a Google Cloud hálózatán belül futott, így a hálózati késleltetés nem befolyásolta a mérést.

## Konfiguráció
* **Cloud Run:** Max 3 instance, max 10 concurrent requests per instance
* **Locust:** 30 user

## Locust workflow
A teszt egy életszerű folyamatot szimulál. Minden szál saját prefixet generál, amivel regisztrál és belép. A feladatok súlyozva vannak a reálisabb terhelés érdekében:
- **ensure_gallery:** Ellenőrzi, van-e élő galéria, ha nincs (induláskor vagy törlés után), létrehoz egyet.
- **list_photos (súly: 10):** A leggyakoribb művelet, ami folyamatosan lekéri a galéria tartalmát.
- **upload_photo (súly: 2):** Új képet tölt fel (test.jpg).
- **delete_everything (súly: 1):** Ritkábban fut le, letörli a galériát, így tesztelve a teljes életciklust.

## Eredmények

A mérés során a Cloud Run példányok száma a terhelés hatására dinamikusan emelkedett. Amikor a kérések száma átlépte a példányonkénti 10-es korlátot, a rendszer elindította a második, majd harmadik konténert is.

![Cloud Run grafikonok](res1.png)

A Locust grafikonján is látszik, hogy a válaszidők stabilak maradtak a teszt alatt, látszódik, ahogy indulásnál elindítja a többi konténert is, ennek hatására van az elején a válaszidőkben egy kiugrás.

![Locust chartok](res3.png)

A statisztika alapján az átlagos válaszidő 500ms körül alakult, ami az adatbázis-műveletek és a képfeltöltések mellett jó teljesítményt jelent.

![Locust statisztika](res2.png)

## Hibák elemzése
A teszt során megjelent pár 401 Unauthorized hiba. Ez valószínűleg az architektúra sajátossága miatt van, mivel az új példányok indulásakor a tokeneket hitelesítő kulcsok szinkronizálása pár másodpercet igénybe vesz. Ezalatt a beérkező kéréseket a rendszer még nem tudja validálni. A többi funkcionális művelet (lista, feltöltés, törlés) a kezdeti bemelegedés után 0 hibával futott le.