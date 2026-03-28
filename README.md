# Felhőalapú Lab – fényképalbum

Full-stack webalkalmazás Angular frontenddel, .NET backenddel és PostgreSQL adatbázissal, Google Cloud platformon deployolva.

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
- Automatikus deploy a `main` branch-ről
- A build folyamatot a Cloud Build végzi

---

## Architektúra

Az alkalmazás három különálló serviceként fut a Google Cloud platformon:

- Frontend (Cloud Run)
- Backend (Cloud Run)
- PostgreSQL adatbázis (Cloud SQL)

A backend Entity Framework Core segítségével kapcsolódik az adatbázishoz.  
A séma migrációk alapján jön létre és frissül a backend indulásával.

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
FrontendUrl=[https://frontend-xxxx.a.run.app](https://frontend-xxxx.a.run.app)
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
