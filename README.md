# Gezlnomics

Containerized full-stack app for a manual "true ledger":
- register period starting balance (salary after tax)
- log transactions
- add recurring rules
- track current balance, forecast balance, and money-per-day KPIs

## Stack
- `traefik` (reverse proxy / single public entrypoint)
- `postgres` (database)
- `backend` (.NET 8 API + EF Core)
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
   - App via Traefik: `http://localhost:3000`
   - API liveness via proxy: `http://localhost:3000/health/live`
   - API readiness via proxy: `http://localhost:3000/health/ready`

## Default commands
```bash
make config   # validate compose
make ps       # status
make logs     # logs
make down     # stop
```

## Environment variables
Core:
- `ASPNETCORE_ENVIRONMENT` (`Development` / `Production`)
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `POSTGRES_PORT`
- `TRAEFIK_HTTP_PORT` (public HTTP port for app)
- `API_PORT` (legacy direct backend port, useful during local transition/debug)
- `FRONTEND_ORIGIN` (must match public app origin)
- `VITE_API_BASE_URL` (keep empty when frontend+api share origin via Traefik)

Auth abuse protection:
- `RATE_LIMIT_AUTH_LOGIN_PERMIT_LIMIT`
- `RATE_LIMIT_AUTH_LOGIN_WINDOW_SECONDS`
- `RATE_LIMIT_AUTH_SIGNUP_PERMIT_LIMIT`
- `RATE_LIMIT_AUTH_SIGNUP_WINDOW_SECONDS`

## Production baseline
Use production values and terminate TLS in front of Traefik or directly in Traefik:
- `ASPNETCORE_ENVIRONMENT=Production`
- `FRONTEND_ORIGIN=https://<your-domain>`
- strong DB credentials via secret store
- HTTPS in front of all auth/session traffic

Important production behavior in backend:
- cookie `SecurePolicy=Always` in production
- CORS requires explicit `Frontend:Origin` in production
- auth endpoints are rate-limited per client IP
- readiness endpoint (`/health/ready`) verifies DB connectivity

## GDPR baseline (technical)
Current app supports key data-subject rights:
- data export: `GET /api/account/export`
- account/data deletion: `DELETE /api/account/`

Before go-live, ensure:
- privacy notice + lawful basis documented
- data retention policy defined (incl. backups)
- data processing agreement with hosting/providers
- incident/breach response process documented
- access control to logs/backups and rotation of secrets

## OpenAPI + frontend types
- Backend exposes Swagger/OpenAPI in `Development`.
- OpenAPI JSON: `http://localhost:3000/swagger/v1/swagger.json` (via Traefik)
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
Repository CI validates:
- backend restore/build/test
- frontend install/build
- `docker compose config`
- generated frontend API types

## Release runbook
See:
- `docs/release-runbook.md`
