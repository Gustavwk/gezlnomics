# Gezlnomics Boilerplate

Containerized boilerplate with a PostgreSQL database, .NET 8 Web API (onion architecture + EF Core migrations), and a React + Vite frontend.

## Prerequisites
- Docker + Docker Compose
- (Optional) `make` for shorter run commands

## Services in Docker Compose
The application runs in a meaningful 3-container setup:
- **postgres**: persistent PostgreSQL database
- **backend**: .NET 8 API with automatic EF Core migration on startup
- **frontend**: built React app served through Nginx

## Quick start
1. Copy env file:
   ```bash
   cp .env.example .env
   ```
2. Start all containers:
   ```bash
   make up
   ```
   or:
   ```bash
   docker compose up --build -d
   ```
3. Open the app:
   - Frontend: `http://localhost:3000`
   - API health: `http://localhost:8080/health`

## Useful commands
```bash
make config   # validate docker compose file
make ps       # show running containers and health
make logs     # stream logs
make down     # stop and remove containers
```

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
