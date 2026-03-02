# Release Runbook

## Preconditions
- CI green on target commit.
- Production `.env.production` prepared from `.env.production.example`.
- DNS `A` record for `APP_HOST` points to droplet.
- Latest backup and restore test completed.

## Required production configuration
- `ASPNETCORE_ENVIRONMENT=Production`
- `APP_HOST=<domain>`
- `ACME_EMAIL=<ops-email>`
- `FRONTEND_ORIGIN=https://<domain>`
- Strong DB credentials (no defaults).

## Deploy
1. Validate config:
   - `docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production config`
2. Start/upgrade:
   - `docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up --build -d`
3. Verify:
   - `GET https://<domain>/health/live` = 200
   - `GET https://<domain>/health/ready` = 200
   - Signup/login/logout with username

## Security checklist
- HTTPS certificate issued and auto-renewing.
- Only ports 22/80/443 open on host firewall.
- Postgres and backend are not exposed publicly.
- Secrets rotated from development defaults.
- Rate limits + login lockout active.

## GDPR checklist
- Privacy notice published.
- Data retention policy set (db, logs, backups).
- DPA signed with DigitalOcean and other processors.
- Data subject operations tested:
  - `GET /api/account/export`
  - `DELETE /api/account/`
- Incident response process documented.

## Incident first response
1. `docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production ps`
2. `docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production logs --tail=200 traefik backend frontend postgres`
3. Contain (rotate secrets, block patterns, rollback if needed).
4. Assess breach obligations and notify when required.

## Rollback
1. Redeploy previous image tags/commit with same compose command.
2. Verify `/health/live` and `/health/ready`.
3. Execute smoke tests.
