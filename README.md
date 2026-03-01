# Gezlnomics

Containerized full-stack app for a manual "true ledger":
- register period starting balance (salary after tax)
- log transactions (actual, planned, savings in/out)
- add recurring rules
- track current balance, forecast balance, and money-per-day

## Stack
- `postgres` (database)
- `backend` (.NET 8 minimal API + EF Core)
- `frontend` (React/Vite built and served by Nginx)

## Run
1. Copy env file:
   ```bash
   cp .env.example .env
   ```
2. Start everything:
   ```bash
   make up
   ```
   or:
   ```bash
   docker compose up --build -d
   ```
3. Open:
   - Frontend: `http://localhost:3000`
   - Health: `http://localhost:8080/health`

## Default commands
```bash
make config   # validate compose
make ps       # status
make logs     # logs
make down     # stop
```

## PowerShell helper (Windows)
For Windows kan du bruge et samlet script, der abstraherer compose-flow og typegenerering:

```powershell
.\scripts\dev.ps1 help
.\scripts\dev.ps1 sync
```

Kommandoer:
- `.\scripts\dev.ps1 config` - valider compose
- `.\scripts\dev.ps1 up` - start stack (`--build -d`)
- `.\scripts\dev.ps1 up -NoBuild` - start uden rebuild
- `.\scripts\dev.ps1 down` - stop stack
- `.\scripts\dev.ps1 ps` - status
- `.\scripts\dev.ps1 logs` - foelg logs
- `.\scripts\dev.ps1 rebuild` - no-cache build
- `.\scripts\dev.ps1 types` - vent paa swagger + generer frontend-typer
- `.\scripts\dev.ps1 sync` - config + up + swagger-wait + typegenerering

## Auth model
MVP uses email/password with cookie-based auth.

## OpenAPI + frontend typer
- Backend eksponerer Swagger/OpenAPI i `Development`.
- OpenAPI JSON findes på: `http://localhost:8080/swagger/v1/swagger.json`.
- Frontend-typer genereres fra OpenAPI og committes i repo.

Kommandoer (køres i `frontend`):
```bash
npm run types:generate   # generer src/generated/api-types.ts fra swagger
npm run types:check      # regenerer + fail hvis generated fil har ændringer
```

Workflow ved API-ændringer:
1. Start backend i development.
2. Kør `npm run types:generate` i frontend.
3. Commit både backend-ændringer og opdateret `src/generated/api-types.ts`.

CI validerer også, at den genererede typefil er opdateret.

Endpoints:
- `POST /api/auth/signup`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`

## Core API
Settings:
- `GET /api/settings/`
- `PUT /api/settings/`

Income periods (startsaldo):
- `GET /api/income-periods/`
- `POST /api/income-periods/`
- `PUT /api/income-periods/{id}`
- `DELETE /api/income-periods/{id}`

Transactions:
- `GET /api/transactions/`
- `POST /api/transactions/`
- `PUT /api/transactions/{id}`
- `DELETE /api/transactions/{id}`

Recurring rules:
- `GET /api/recurring-rules/`
- `POST /api/recurring-rules/`
- `PUT /api/recurring-rules/{id}`
- `DELETE /api/recurring-rules/{id}`

Ledger:
- `GET /api/ledger/summary?asOf=YYYY-MM-DD`
- `GET /api/ledger/timeline?from=YYYY-MM-DD&to=YYYY-MM-DD`

## Ledger logic
- Period is payday-to-payday (configured in user settings).
- `Current balance` = period starting balance + signed actual transactions up to today.
- `Forecast balance` = current balance + future transactions + recurring occurrences until period end.
- `Money/day` = `max(0, forecast balance) / max(1, days until next payday)`.

## Notes
- All finance data is manual input by design (no bank sync/import in MVP).
- EF migrations run on backend startup via hosted service.
