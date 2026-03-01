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

## Auth model
MVP uses email/password with cookie-based auth.

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
