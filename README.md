# Gezlnomics

Containerized full-stack app for a manual "true ledger":
- register period starting balance (salary after tax)
- log transactions
- add recurring rules
- track current balance, forecast balance, and money-per-day KPIs

## Hvad kan appen bruges til?
Gezlnomics er en personlig budget- og likviditetsapp til dig, der vil have et simpelt overblik over din måned uden bankintegration.

Du kan bruge den til at:
- oprette bruger og logge ind
- sætte startsaldo for lønperioder
- registrere daglige udgifter/transaktioner med note
- oprette faste udgifter (tilbagevendende poster)
- skifte mellem perioder og oprette næste periode
- se KPI-overblik:
  - nuværende saldo
  - penge pr. dag (før dagens forbrug)
  - penge pr. dag fremadrettet
  - forbrug i dag (uden faste udgifter)
  - dage til næste løn

Den er især nyttig til:
- daglig styring af rådighedsbeløb
- at undgå at bruge for meget tidligt i perioden
- at forstå forskellen mellem faktiske udgifter og faste planlagte udgifter

## Stack
- `postgres` (database)
- `backend` (.NET 8 minimal API + EF Core)
- `frontend` (React/Vite built and served by Nginx)

## Local Development
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
   - API health (legacy): `http://localhost:8080/health`
   - API liveness: `http://localhost:8080/health/live`
   - API readiness: `http://localhost:8080/health/ready`

## Default commands
```bash
make config   # validate compose
make ps       # status
make logs     # logs
make down     # stop
```

## PowerShell helper (Windows)
```powershell
.\scripts\dev.ps1 help
.\scripts\dev.ps1 sync
```

Commands:
- `config` - validate compose
- `up` - start stack (`--build -d`)
- `up -NoBuild` - start without rebuild
- `down` - stop stack
- `ps` - status
- `logs` - tail logs
- `rebuild` - no-cache build
- `types` - wait for swagger + generate frontend types
- `sync` - config + up + swagger wait + type generation

## Production Baseline
Use the same containers, but configure production values:
- `ASPNETCORE_ENVIRONMENT=Production`
- `FRONTEND_ORIGIN=https://<your-frontend-domain>`
- secure secrets for DB credentials
- HTTPS termination in front of backend/frontend

Important production behavior in backend:
- cookie `SecurePolicy=Always` in production
- CORS requires explicit `Frontend:Origin` in production
- Swagger UI only in `Development`
- readiness endpoint (`/health/ready`) verifies DB connectivity

## Auth model
MVP uses email/password with cookie-based auth.

## OpenAPI + frontend types
- Backend exposes Swagger/OpenAPI in `Development`.
- OpenAPI JSON: `http://localhost:8080/swagger/v1/swagger.json`
- Frontend types are generated from OpenAPI and committed.

Run in `frontend`:
```bash
npm run types:generate
npm run types:check
```

Workflow for API changes:
1. Start backend in development.
2. Run `npm run types:generate` in frontend.
3. Commit backend changes + updated `src/generated/api-types.ts`.

## CI Gates
Repository CI now validates:
- backend restore/build/test
- frontend install/build
- `docker compose config`
- generated frontend API types (separate workflow)

## Core API
Settings:
- `GET /api/settings/`
- `PUT /api/settings/`

Income periods:
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

## Release Runbook
See:
- `docs/release-runbook.md`

