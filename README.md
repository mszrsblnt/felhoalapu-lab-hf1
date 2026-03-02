# Felhőalapú Lab – fényképalbum

Full-stack webalkalmazás Angular frontenddel, .NET backenddel és PostgreSQL adatbázissal, Render.com PaaS platformon deployolva.

---

## Tech Stack

**Frontend**
- Angular 21
- Bootstrap  

**Backend**
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core (Code First + Migrations)
- Dockerizált

**Database**
- PostgreSQL

**Cloud / Deployment**
- Render.com
- Automatikus deploy a `main` branch-ről
- A build folyamatot a Render végzi

---

## Architektúra

Az alkalmazás három különálló serviceként fut a Render platformon:

- Frontend
- Backend
- PostgreSQL adatbázis

A backend Entity Framework Core segítségével kapcsolódik az adatbázishoz.  
A séma migrációk alapján jön létre és frissül a backend indulásával.

---

## Környezeti változók (Backend)

A backend működéséhez az alábbi változók definiálása szükséges éles környezetben:

| Név | Leírás |
|------|--------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL adatbázis connection string |
| `FrontendUrl` | A frontend publikus URL-je (CORS konfigurációhoz) |

Példa:

```env
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...
FrontendUrl=https://your-frontend-url.onrender.com
```

## Környezeti változók (Frontend)

A frontend az alábbi változót használja a backend API eléréséhez:

| Név | Leírás |
|------|--------|
| `ApiUrl` | A backend publikus URL-je |

Példa:

```env
ApiUrl=https://your-backend-url.onrender.com
