# Release Runbook

## Preconditions
- CI is green on the commit to deploy.
- Docker images are built from that commit.
- `.env` / secret store contains production values.
- Latest DB backup completed successfully.

## Required production configuration
- `ASPNETCORE_ENVIRONMENT=Production`
- `FRONTEND_ORIGIN=https://<app-domain>`
- `TRAEFIK_HTTP_PORT` bound behind TLS endpoint/load balancer
- DB credentials via secrets
- `VITE_API_BASE_URL=` (empty for same-origin through Traefik)

## Security controls before go-live
- Auth rate limits tuned for production traffic.
- HTTPS enforced end-to-end for user traffic.
- Session cookie marked `Secure` (automatic in production).
- Secrets rotated from development defaults.
- Backup encryption + restore test documented.

## GDPR controls before go-live
- Privacy notice and lawful basis documented.
- Data retention period defined (DB + backups + logs).
- DPA signed with hosting/processors.
- Data subject request flow verified:
  - export endpoint: `GET /api/account/export`
  - deletion endpoint: `DELETE /api/account/`
- Breach-response process with contact path documented.

## Deploy steps
1. Validate compose config:
   - `docker compose config`
2. Pull/build target images.
3. Start stack:
   - `docker compose up --build -d`
4. Verify health through Traefik:
   - `GET /health/live` returns 200
   - `GET /health/ready` returns 200
5. Verify functional smoke:
   - signup/login/logout
   - create transaction
   - open ledger summary

## Rollback steps
1. Switch to previous known-good image tags.
2. `docker compose up -d` with previous tags.
3. Verify `live` and `ready` endpoints.
4. Confirm app smoke flows.

## Incident first response
1. Check container status:
   - `docker compose ps`
2. Inspect logs:
   - `docker compose logs --tail=200 traefik backend frontend postgres`
3. If DB connectivity fails:
   - verify DB credentials/host/network
   - confirm Postgres health
4. If startup migration fails:
   - halt rollout
   - restore previous image

## Go-live checklist
- [ ] CI green
- [ ] Compose config valid
- [ ] Production env vars/secrets set
- [ ] Health endpoints green
- [ ] Smoke tests green
- [ ] Backup recent and restore-tested
- [ ] GDPR checklist completed
- [ ] Rollback command path verified
