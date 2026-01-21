# Gezlnomics Boilerplate

Containerized boilerplate with a PostgreSQL database, .NET 8 Web API (onion architecture + EF Core migrations), and a React + Vite frontend.

## Prerequisites
- Docker + Docker Compose

## How to run
1. Ensure the `.env` file in the repo root has values that work for your machine.
2. Start everything:
   ```bash
   docker compose up --build
   ```
3. Open the frontend at `http://localhost:3000` (default).

## Backend architecture
The backend follows a strict onion architecture:
- **Backend.Domain**: entities only (no dependencies)
- **Backend.Application**: interfaces + application services (depends on Domain)
- **Backend.Infrastructure**: EF Core + gateway implementations (depends on Domain + Application)
- **Backend.Api**: presentation layer (depends on Application only)

The API dynamically loads Infrastructure at runtime to keep project references aligned with the dependency rules.

## Migrations
Migrations live in **Backend.Infrastructure**.

### Add a new migration (locally)
```bash
cd backend

dotnet ef migrations add AddSomething \
  --project src/Backend.Infrastructure \
  --startup-project src/Backend.Api
```

### Apply migrations (locally)
```bash
cd backend

dotnet ef database update \
  --project src/Backend.Infrastructure \
  --startup-project src/Backend.Api
```

### Apply migrations in Docker
Migrations are applied automatically on API startup via a hosted service. If you need to run it manually, rebuild the API container:
```bash
docker compose up --build backend
```

## Troubleshooting
- **Ports already in use:** update `API_PORT` or `FRONTEND_PORT` in `.env`.
- **Database connection failures:** verify the `.env` values match the Postgres container settings.
- **Migration issues:** delete the volume `postgres_data` and restart:
  ```bash
  docker compose down -v
  docker compose up --build
  ```
