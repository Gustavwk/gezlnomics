# AGENTS.md

## Formål
Dette repo er en containeriseret full-stack demo med tre services:
1. `postgres` (databasen)
2. `backend` (.NET 8 API)
3. `frontend` (React/Vite bygget og serveret via Nginx)

Målet er, at hele applikationen kan startes med Docker Compose uden lokal installation af .NET/Node.

## Arbejdsgang for agenter
- Brug primært `docker compose` som den officielle måde at køre projektet på.
- Hold arkitekturen med separate containere intakt (database, API, frontend).
- Opdater altid `README.md`, hvis startup-flow, porte eller miljøvariabler ændres.
- Tilføj/vedligehold `Makefile` targets, hvis det gør drift og onboarding enklere.

## Standardkommandoer
- Start alt: `make up`
- Stop alt: `make down`
- Følg logs: `make logs`
- Se status/health: `make ps`

Hvis `make` ikke er tilgængelig, brug:
- `docker compose up --build -d`
- `docker compose down`

## Kvalitetstjek før commit
1. `docker compose config` skal være valid.
2. Dokumentation skal matche faktisk konfiguration (`.env`, porte, services).
3. Nye filer skal have meningsfulde navne og kort forklaring i README ved behov.
