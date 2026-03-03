# GDPR Operations Checklist

## Governance
- [ ] Controller details documented and published.
- [ ] Record of processing activities maintained.
- [ ] Processor agreements (DPA) signed for all vendors.

## Product/Data
- [ ] Data model reviewed for minimization (username, no email required).
- [ ] Retention policy defined (DB, logs, backups).
- [ ] Export and deletion flow tested end-to-end.

## Security
- [ ] HTTPS enabled and auto-renewing certificates verified.
- [ ] Secrets rotated from defaults.
- [ ] Public ports restricted to `80/443` (+ `22` for SSH).
- [ ] Auth abuse protections verified (rate limit + lockout).

## Incident Response
- [ ] Breach response owner identified.
- [ ] Escalation path and contact matrix documented.
- [ ] 72-hour assessment workflow documented.

## Backup/Restore
- [ ] Daily backup + PITR configured.
- [ ] Restore tested in non-production.
- [ ] Backup retention and encryption documented.

