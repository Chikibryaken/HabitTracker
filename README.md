# HabitTracker

A full-stack habit tracker with JWT-based authentication, built with .NET 10 and React.

## About

HabitTracker lets a user register, create daily or weekly habits, mark them done, and see progress over time — current streak, monthly completion rate, and a GitHub-style activity calendar. It was built as a portfolio project to demonstrate a complete, production-shaped slice of full-stack work: a layered .NET backend with a relational schema and EF Core migrations, cookie-free JWT authentication with rotating refresh tokens, and a React + TypeScript frontend where all server state goes through TanStack Query (no ad-hoc `useState` mirrors of API data).

[Live demo](#) · [API](#)

## Screenshots

| Dashboard | Habit detail & calendar | Login |
|---|---|---|
| ![Dashboard](docs/screenshots/dashboard.png) | ![Habit detail](docs/screenshots/habit-detail.png) | ![Login](docs/screenshots/login.png) |

## Tech Stack

**Backend**
- .NET 10 / ASP.NET Core Minimal API
- Entity Framework Core 10 + Npgsql (PostgreSQL)
- FluentValidation
- xUnit

**Frontend**
- React 19 + TypeScript
- Vite
- TanStack Query
- Zustand
- Axios
- React Router

**Deploy**
- Docker (multi-stage build for the API)
- Railway (API + PostgreSQL)
- Vercel (frontend)

## Features

- Email/password registration and login
- JWT access tokens with rotating, revocable refresh tokens (hashed at rest, one-time use)
- Habit CRUD (create, edit, archive, delete), scoped per authenticated user
- Daily or weekly habit frequency
- Idempotent "mark done" / "unmark" for a specific date
- Per-habit stats: current streak, monthly completion rate, days completed vs. days elapsed
- Activity calendar per habit — click a past day to backfill a missed entry, navigate between months
- Dashboard with streak badges per habit card
- User profile page (email, join date, active habit count)

## Architecture

The backend follows a layered structure so business rules stay independent of EF Core, ASP.NET Core, and each other's implementation details — the `Application` layer only depends on `Domain` and abstractions it defines itself (`IApplicationDbContext`, `ITokenService`, ...), which `Infrastructure` implements. This keeps the core logic (validation, streak/percentage calculation, token issuance rules) unit-testable without a database or an HTTP pipeline.

```
src/
├── HabitTracker.Domain            # Entities, enums — no dependencies on other layers
├── HabitTracker.Application       # DTOs, service interfaces + implementations, FluentValidation validators
├── HabitTracker.Infrastructure    # EF Core DbContext & configurations, JWT issuing, password/token hashing
└── HabitTracker.Api               # Minimal API endpoint definitions, DI composition, Program.cs
```

```
client/src/
├── api/               # axios instance (+ auth interceptors), typed request functions
├── features/habits/   # TanStack Query hooks and habit-specific components
├── components/        # shared UI (Modal, ProtectedRoute)
├── pages/             # route-level components
├── store/             # Zustand auth store (tokens, user, derived isAuthenticated)
└── types/             # shared TypeScript types
```

## Key Technical Decisions & Trade-offs

- **Refresh tokens are hashed (SHA-256) at rest and rotated on every use.** Each refresh call revokes the old token and issues a new pair, and logout revokes the token explicitly. Reuse detection (auto-revoking a whole token family if a already-used token is replayed) is **not** implemented — see Possible Improvements.
- **Access and refresh tokens are stored in `localStorage` on the client**, not `httpOnly` cookies. This is simpler for a single-frontend pet project and avoids CORS/cookie complications from the API (Railway) and frontend (Vercel) living on different domains — but it is more exposed to XSS than an `httpOnly` cookie would be.
- **Stats (streak, monthly completion rate) are computed on every request from raw `HabitCompletion` rows**, not denormalized/cached. Simpler and always correct, at the cost of recomputing on each read — acceptable at pet-project scale.
- **"Today" is the client's local date**, sent explicitly to the API (e.g. `?today=2026-07-02`) rather than derived from the server clock, so streaks make sense in the user's own timezone. The server validates that the date isn't in the future, but there's no server-side timezone awareness beyond that — a user traveling across timezones mid-day could see edge-case behavior.
- **Weekly-frequency habits use a simplified v1 model**: a streak counts consecutive ISO weeks with at least one completion, and the monthly rate is the fraction of the month's elapsed weeks with a completion. It intentionally doesn't try to handle habits with a target count greater than 1 per period.
- **Database migrations run automatically on startup in Production** (`Program.cs`, wrapped in try/catch with logging). This keeps deployment to a single Railway service simple, but it's a known compromise: with more than one instance running concurrently, two instances migrating at once can race. A real multi-instance deployment should run migrations as a separate release step instead.

## Getting Started (local)

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS)
- [Docker](https://www.docker.com/) (for local PostgreSQL)

### 1. Start PostgreSQL

```bash
docker compose up -d
```

This starts `postgres:16` on port `5432` with database/user/password all set to `habittracker` (see `docker-compose.yml`).

### 2. Configure the API

Create `src/HabitTracker.Api/appsettings.Development.json` (gitignored — not committed) with:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=habittracker;Username=habittracker;Password=habittracker"
  },
  "Jwt": {
    "Issuer": "HabitTracker.Api",
    "Audience": "HabitTracker.Client",
    "Key": "<a random string, at least 32 bytes, base64-encoded works well>",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  }
}
```

### 3. Apply migrations and run the API

```bash
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef database update --project src/HabitTracker.Infrastructure --startup-project src/HabitTracker.Api
dotnet run --project src/HabitTracker.Api
```

The API listens on `http://localhost:5290` (and `https://localhost:7289`) by default — see `src/HabitTracker.Api/Properties/launchSettings.json`. Swagger UI is available at `/swagger` in Development.

### 4. Run the frontend

```bash
cd client
npm install
npm run dev
```

Open `http://localhost:5173`. The Vite dev server proxies `/api` requests to the backend (`vite.config.ts`).

## Environment Variables

### Backend

| Variable | Used for | Local (Development) | Production (Railway) |
|---|---|---|---|
| `ConnectionStrings__Postgres` | Postgres connection string | Set in `appsettings.Development.json` | Optional — takes priority over `DATABASE_URL` if set |
| `DATABASE_URL` | Postgres connection URL (`postgresql://user:pass@host:port/db`) | Not used | Reference to Railway's Postgres plugin; parsed into a Npgsql connection string automatically |
| `Jwt__Issuer` | JWT `iss` claim | `appsettings.Development.json` | Required |
| `Jwt__Audience` | JWT `aud` claim | `appsettings.Development.json` | Required |
| `Jwt__Key` | JWT signing key (HMAC-SHA256, ≥32 bytes) | `appsettings.Development.json` | Required — use a different value than local |
| `Jwt__AccessTokenMinutes` | Access token lifetime | `appsettings.Development.json` | Required |
| `Jwt__RefreshTokenDays` | Refresh token lifetime | `appsettings.Development.json` | Required |
| `FRONTEND_URL` | Allowed CORS origin (in addition to `localhost:5173`) | Not needed | Set to the deployed Vercel URL |
| `PORT` | Port Kestrel binds to (`0.0.0.0:$PORT`) | Not needed | Set automatically by Railway |
| `ASPNETCORE_ENVIRONMENT` | Enables production behavior (auto-migrate, disables Swagger) | `Development` (via `launchSettings.json`) | `Production` |

### Frontend

| Variable | Used for | Local | Production (Vercel) |
|---|---|---|---|
| `VITE_API_URL` | Base URL for API requests | Unset — falls back to `/api` (Vite proxy) | Full API URL, e.g. `https://<railway-domain>/api` |

## API Overview

All routes are prefixed with `/api`. Authenticated routes expect `Authorization: Bearer <accessToken>`.

| Method | Path | Purpose | Auth |
|---|---|---|---|
| POST | `/auth/register` | Create an account, returns token pair | No |
| POST | `/auth/login` | Log in, returns token pair | No |
| POST | `/auth/refresh` | Exchange a refresh token for a new pair | No |
| POST | `/auth/logout` | Revoke a refresh token | Yes |
| GET | `/auth/me` | Current user's profile + habit count | Yes |
| GET | `/habits` | List habits (`?includeArchived=`) | Yes |
| GET | `/habits/{id}` | Get a single habit | Yes |
| POST | `/habits` | Create a habit | Yes |
| PUT | `/habits/{id}` | Update a habit | Yes |
| PUT | `/habits/{id}/archive` | Archive a habit | Yes |
| DELETE | `/habits/{id}` | Delete a habit | Yes |
| GET | `/habits/{id}/stats` | Streak & monthly completion rate (`?today=`) | Yes |
| GET | `/habits/stats` | Stats summary across all active habits | Yes |
| POST | `/habits/{habitId}/complete` | Mark a date done (idempotent) | Yes |
| DELETE | `/habits/{habitId}/complete` | Unmark a date | Yes |
| GET | `/habits/{habitId}/completions` | List completions in a date range | Yes |
| GET | `/health` | Health check | No |

## Testing

```bash
dotnet test tests/HabitTracker.Tests
```

Covers `HabitStatsCalculator` — the pure functions behind streak and monthly-completion-rate math — with 16 unit tests over edge cases: empty completion sets, gaps that break a streak, a missing "today" that doesn't break it, unsorted input, and daily/weekly dispatch.

## Possible Improvements

- Refresh-token reuse detection (revoke the full token family if an already-used token is replayed)
- `httpOnly` cookie-based token storage instead of `localStorage`
- Server-side timezone awareness beyond "reject future dates"
- Have the dashboard use the existing `GET /habits/stats` aggregate endpoint instead of one stats request per habit card
- Localization (UI is English-only)
