# Felhőalapú Lab – fényképalbum

Full-stack webalkalmazás Angular frontenddel, .NET backenddel és PostgreSQL adatbázissal, Google Cloud platformon terraformmal deployolva.

Publikálva: [Cloud Gallery App](https://photo-gallery-frontend-274894838244.europe-west1.run.app/home)

---

## Előfeltételek (Google Cloud beállítások)

Mielőtt az automatizáció elindulna, az alábbi feltételeknek teljesülniük kell Google Cloud oldalon:

### 1. Terraform State tárolása
A Terraform állapotfájlját (`terraform.tfstate`) egy **Google Cloud Storage Bucket**-ben tároljuk.
- **Szükséges:** Egy tetszőleges nevű bucket (pl. `projekt-id-tfstate`).
- **Konfiguráció:** A `main.tf` fájlban a `backend "gcs"` blokknak erre kell mutatnia.

### 2. IAM Jogosultságok
A GitHub Actions által használt Service Accountnak (amelynek kulcsa a `GCP_CREDENTIALS` secretben van) az alábbi szerepkörökkel kell rendelkeznie:
- **Editor:** Általános erőforrás-kezeléshez és a legtöbb Google Cloud komponens létrehozásához.
- **Cloud Run Admin:** Teljes körű jogosultság a Cloud Run szolgáltatások kezeléséhez és konfigurálásához.
- **Project IAM Admin:** Szükséges ahhoz, hogy a Terraform módosíthassa az IAM policy-ket (pl. a szolgáltatások nyilvánossá tétele az allUsers tagon keresztül).
---

## Tech Stack

**Frontend**
- Angular 21
- Bootstrap  
- Dockerizált (Nginx)

**Backend**
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core (Code First + Migrations)
- Dockerizált

**Database**
- PostgreSQL (Cloud SQL)

**Cloud / Deployment**
- Google Cloud Platform (Cloud Run)
- Automatikus deploy az `iac-terraform` branch-ről
- GitHub Actions CI/CD pipeline (Github build, Google registry)
- Terraform (Infrastructure as Code)

---

## Architektúra

Az alkalmazás három különálló serviceként fut a Google Cloud platformon:

- Frontend (Cloud Run)
- Backend (Cloud Run)
- PostgreSQL adatbázis (Cloud SQL)

A backend Entity Framework Core segítségével kapcsolódik az adatbázishoz.  
A séma migrációk alapján jön létre és frissül a backend indulásával.

---

## Infrastructure as Code (IaC)

A projekt infrastruktúrája **Terraform** segítségével lett létrehozva.

### Konfigurált komponensek
- **Cloud SQL:** PostgreSQL példány, adatbázis és dedikált DB-felhasználó létrehozása.
- **Cloud Run Services:** A frontend és backend konténerek konfigurálása, környezeti változók (connection stringek, URL-ek) automatikus injektálása.
- **IAM Policyk:** A szolgáltatások nyilvános elérhetőségének (`allUsers`) és a Cloud SQL proxy hozzáférésének automatizált beállítása.

### CI/CD Automatizáció (GitHub Actions)
A telepítési folyamat teljesen automatizált a `iac-terraform` ágon keresztül:

1. **Intelligens szűrés:** A rendszer csak akkor buildeli újra a Docker image-eket, ha a kód valóban változott a `CloudGalleryApi` vagy `cloud-gallery-fe` mappákban.
2. **Image Digest alapú futtatás:** A Terraform nem a bizonytalan `:latest` taget használja, hanem a build során generált egyedi SHA256 ujjlenyomatot (digest). Ez garantálja, hogy a Cloud Run minden kódváltozás után azonnal frissül.
3. **Fallback logika:** Ha törlődnek az image-ek a Google Artifact Registry-ből, a pipeline felismeri a hiányt és kódváltozás nélkül is újrabuildeli és pusholja az imageket.

---

## Környezeti változók (Backend)

A backend működéséhez az alábbi változók definiálása szükséges éles környezetben a Cloud Run-on:

| Név | Leírás |
|------|--------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL adatbázis connection string (Cloud SQL Unix Socket) |
| `FrontendUrl` | A frontend publikus URL-je (CORS konfigurációhoz) |

Példa:

```env
ConnectionStrings__DefaultConnection=Host=/cloudsql/PROJEKT_ID:REGION:INSTANCE_ID;Database=...;Username=...;Password=...
FrontendUrl=https://frontend-xxxx.a.run.app
```

## Környezeti változók (Frontend)

A frontend az alábbi változót használja a backend API eléréséhez:

| Név | Leírás |
|------|--------|
| `API_URL` | A backend publikus URL-je |

Példa:

```env
API_URL=[https://backend-xxxx.a.run.app](https://backend-xxxx.a.run.app)
```
