# Release Runbook (MVP)

## Preconditions
- CI is green on the commit to deploy.
- Docker images are built from that commit.
- `.env` / secret store contains production values.
- Latest DB backup completed successfully.

## Required production configuration
- `ASPNETCORE_ENVIRONMENT=Production`
- `FRONTEND_ORIGIN=https://<frontend-domain>`
- DB credentials via secrets
- `VITE_API_BASE_URL=https://<api-domain>`

## Deploy steps
1. Validate config:
   - `docker compose config`
2. Pull/build target images.
3. Start stack:
   - `docker compose up --build -d`
4. Verify health:
   - `GET /health/live` returns 200
   - `GET /health/ready` returns 200
5. Verify functional smoke:
   - login/signup
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
   - `docker compose logs --tail=200 backend frontend postgres`
3. If DB connectivity fails:
   - verify DB credentials/host/network
   - confirm Postgres health
4. If startup migration fails:
   - halt rollout
   - restore previous image

## Backup and restore checklist
- Daily backup schedule enabled.
- Retention policy defined.
- Restore tested in non-production environment.
- Restore test date documented.

## Go-live checklist
- [ ] CI green
- [ ] Compose config valid
- [ ] Production env vars/secrets set
- [ ] Health endpoints green
- [ ] Smoke tests green
- [ ] Backup recent and restore-tested
- [ ] Rollback command path verified
