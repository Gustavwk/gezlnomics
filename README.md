# Gezlnomics

Containerized full-stack app for a manual "true ledger":
- register period starting balance (salary after tax)
- log transactions
- add recurring rules
- track current balance, forecast balance, and money-per-day KPIs

## Stack
- `traefik` (reverse proxy)
- `postgres` (database)
- `backend` (.NET 8 API + EF Core)
- `frontend` (React/Vite built and served by Nginx)

## Local development
1. Copy env file:
   ```bash
   cp .env.example .env
   ```
2. Start stack:
   ```bash
   make up
   ```
3. Open:
   - App: `http://localhost:3000`
   - Liveness: `http://localhost:3000/health/live`
   - Readiness: `http://localhost:3000/health/ready`

## Default commands
```bash
make config
make up
make down
make logs
make ps
```

## Production deploy (single droplet)
Use compose overrides and production env file:
```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up --build -d
```

Production files:
- `.env.production.example` (template)
- `docker-compose.prod.yml` (HTTPS + ACME + closed internal ports)
- `docker-compose.override.yml` (dev-only direct port bindings for backend/postgres)

## Environment variables
Core:
- `ASPNETCORE_ENVIRONMENT`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `POSTGRES_PORT` (dev convenience)
- `TRAEFIK_HTTP_PORT` (dev convenience)
- `API_PORT` (legacy dev/debug convenience)
- `FRONTEND_ORIGIN`
- `VITE_API_BASE_URL`

Production-only:
- `APP_HOST`
- `ACME_EMAIL`

Auth abuse protection:
- `RATE_LIMIT_AUTH_LOGIN_PERMIT_LIMIT`
- `RATE_LIMIT_AUTH_LOGIN_WINDOW_SECONDS`
- `RATE_LIMIT_AUTH_SIGNUP_PERMIT_LIMIT`
- `RATE_LIMIT_AUTH_SIGNUP_WINDOW_SECONDS`
- `LOGIN_SECURITY_MAX_FAILED_ATTEMPTS`
- `LOGIN_SECURITY_ATTEMPT_WINDOW_SECONDS`
- `LOGIN_SECURITY_LOCKOUT_DURATION_SECONDS`

## Auth model
- Username + password (no email required).
- Cookie-based auth session.
- Login/signup rate-limited by IP.
- Additional lockout on repeated failed login attempts (username + IP).

## OpenAPI + frontend types
- OpenAPI JSON (dev): `http://localhost:3000/swagger/v1/swagger.json`
- Generate frontend types:
  ```bash
  cd frontend
  npm run types:generate
  npm run types:check
  ```

## Backup + restore helpers
- Backup:
  ```bash
  ./scripts/backup-postgres.sh
  ```
- Restore:
  ```bash
  ./scripts/restore-postgres.sh ./backups/<file>.sql.gz
  ```

## GDPR / legal ops docs
- `docs/release-runbook.md`
- `docs/gdpr-operations-checklist.md`
- `docs/privacy-notice-template.md`
- `docs/incident-response-playbook.md`
