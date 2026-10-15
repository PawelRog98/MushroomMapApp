# MushroomMapApp

Full-stack app for mapping mushroom finds on an interactive map (Leaflet/PostGIS). Users drop markers with photos and descriptions, react to locations, and manage the community through a permission-based admin dashboard.

**Features:** interactive map (create/edit/delete markers with photos) · JWT auth with email verification and refresh tokens · profiles with avatars · saved "my localization" (pick on a mini-map in the profile, powers the homepage weather panel via Open-Meteo) · reactions · granular permission system · admin dashboard (suspensions, permissions) · background jobs (Hangfire), thumbnails, email.

## Tech stack

| Layer | Technologies |
|---|---|
| **Frontend** | React 19, TypeScript, Vite 8, Tailwind CSS 4, React Router 7, TanStack Query 5, Zustand, Axios, React Hook Form + Zod, Leaflet / react-leaflet |
| **Backend** | ASP.NET Core Minimal API (.NET 10), MediatR (CQRS), FluentValidation, EF Core + Npgsql, JWT Bearer |
| **Data** | PostgreSQL + PostGIS, Redis (cache + permission cache), Hangfire (jobs + dashboard) |
| **Other** | MailKit (email), SixLabors.ImageSharp (thumbnails), Open-Meteo (weather API), Swashbuckle (Swagger), DotNetEnv, FluentMigrator (separate `MushroomMapApp.Migrator` project) |
| **Infra** | Docker Compose (API + PostgreSQL + Redis) |

## Project structure

The solution (`MushroomMapApp.sln`) has three source projects plus tests: the API (`src/MushroomMapApp`), a standalone FluentMigrator app (`src/MushroomMapApp.Migrator`), the React SPA (`src/MushroomMapApp.Web`), and `tests/` (unit + integration). Helper scripts live in `tools/`.

**Backend — Minimal API + vertical slices.** The API is an ASP.NET Core Minimal API: no controllers, just route groups of lightweight delegates mapped in `Program.cs` (`api/users`, `api/locations`, …). Each feature owns a single `Endpoints.cs` that maps its routes; delegates resolve the current user from `ClaimsPrincipal` and dispatch a MediatR command/query. Business logic is grouped **vertically by feature** under `Features/` (`Users/`, `Locations/`, `Reactions/`, `Weather/`, `Files/`): everything one operation needs — command/query, request record, DTO, validators — sits in one folder per operation (e.g. `CreateLocation/`), instead of horizontal layers. Horizontal concerns are shared: `Domain/` (entities, `AppDbContext`, permissions, seeding), `Infrastructure/` (auth, email, files, Redis, Hangfire, external API clients), and `Features/Common/` (MediatR pipeline behaviors, pagination, response envelopes).

**Migrator.** The API never migrates the database itself. `MushroomMapApp.Migrator` is a small console app that runs FluentMigrator scripts from `Migrations/` (`migrate`, `rollback <n>`, `rollbackall`); the integration tests reuse the same scripts.

**Frontend.** The React SPA mirrors the same idea: shared UI, routing and stores at the top level, while everything feature-specific (API calls, TanStack Query hooks, types, components) lives inside `src/features/*`.

**Tests.** Unit tests cover handlers/validators with an in-memory DbContext; integration tests boot the real app via `WebApplicationFactory` with Testcontainers (PostGIS + Redis), apply migrations and seed automatically. Both mirror the API's feature folders.

**API surface:**

- `api/users` – register, login, refresh, logout, activate-account, create-verification-token, profile, avatar, permissions, suspensions, create-user-location / get-my-location (saved "my localization")
- `api/locations` – create/get/update/delete (bbox query for the map)
- `api/reactions` – add/get reactions, reaction types
- `api/weather` – get-weather (current conditions by lat/lng, cached in Redis)
- `api/files` – get image by id

**Swagger UI** at `/swagger` (Development only) and the **Hangfire dashboard** at `/hangfire` (credentials from `HANGFIRE_USERNAME` / `HANGFIRE_PASSWORD`).

## Running locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+
- [Docker](https://docs.docker.com/get-docker/) (or Podman) with Compose

### 1. Configure environment

```bash
cp .env.example .env     # fill in passwords, JWT secret, SMTP
```

> `JWT_KEY` must be at least **32 characters** — the API refuses to start without it.
> CORS origins come from `CORS_URLS` (comma-separated); include `http://localhost:5173` for the local frontend.
> The frontend defaults to `http://localhost:5000/api`; override it with `VITE_API_URL` in `src/MushroomMapApp.Web/.env`.

### 2. Start infrastructure

Everything at once (API on `:8080`):

```bash
docker compose up --build
```

Databases only, API running locally (recommended while coding):

```bash
docker compose up mushroommapapp.db mushroommapapp.redis
dotnet run --project src/MushroomMapApp --launch-profile LinuxLocal   # listens on :5000
```

### 3. Run database migrations

```bash
dotnet run --project src/MushroomMapApp.Migrator -- migrate
# or: ./tools/migrator-linux.sh   →  1) Migration
```

The connection string is `SQL_CONN` from `.env` (or pass it as the first argument).
Commands: `migrate`, `rollback <n>`, `rollbackall`.
On first API start `DbSeeder` creates roles, permissions, reaction types and the admin account — in a clean environment, **migrate before starting the API**.

### 4. Start the frontend

```bash
cd src/MushroomMapApp.Web
npm install
npm run dev
```

Open **http://localhost:5173**.

### 5. URLs and seeded admin

| Service | URL |
|---|---|
| Frontend | http://localhost:5173 |
| API (local dev / Docker) | http://localhost:5000 / http://localhost:8080 |
| Swagger (Development) / Hangfire | http://localhost:5000/swagger · http://localhost:5000/hangfire |

| Seeded admin | |
|---|---|
| Email | `admin1@admin.com` |
| Password | `Password-1` |

Change these outside local development.

## Tests

```bash
dotnet test                                                  # everything
dotnet test tests/MushroomMap.UnitTests                      # unit only
dotnet test tests/MushroomMap.IntegrationTests               # integration only
```

- **Unit tests** – command handlers, validators, middlewares (no infrastructure).
- **Integration tests** – real HTTP calls against the app (`WebApplicationFactory`) with PostGIS + Redis started by **Testcontainers**, migrations applied and seeded automatically. Requires a running Docker/Podman daemon; no local database is needed. Email is replaced by a fake service, so no SMTP access is required.
- Tests mirror the API's feature folders (`Features/Users`, `Locations`, `Reactions`, `Weather`, `Files`) and cover every endpoint, including validation and permission failures.

## Build

```bash
cd src/MushroomMapApp.Web && npm run build     # type-check + production bundle → dist/
dotnet publish src/MushroomMapApp -c Release -o publish
```

| Frontend script | |
|---|---|
| `npm run dev` / `preview` | Vite dev server (5173) / preview build |
| `npm run build` | `tsc -b` + production build |
| `npm run lint` | ESLint |

## Notes

- Thumbnails are served from `/thumbnails` (files under `Storage/Files`).
- Access tokens refresh automatically on 401 (Axios interceptor); permissions are reloaded after refresh.
- Route and sidebar visibility are permission-driven (`RouteGuard` + `useHasPermission`).
- Successful responses are wrapped in an envelope (`{ value: { success, data, ... } }`); the frontend unwraps it, and so do the integration tests.
- Users save a single "my localization" on their profile (click **Change**, pick a point on the map, save — upserted via `create-user-location`). The homepage weather floating panel reads it through `get-my-location` and links to the profile when none is set.
