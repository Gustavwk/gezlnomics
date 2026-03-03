# Privacy Notice Template (Username-based MVP)

Use this as a starting point and have it legally reviewed before publishing.

## 1. Data Controller
- Company/legal entity:
- Contact email:
- Address:

## 2. What data we process
- Account data: `username`, password hash, created/updated timestamps.
- App data: settings, income periods, transactions, recurring rules.
- Technical data: security/access logs, IPs for abuse prevention.

## 3. Purpose and legal basis
- Provide the budgeting service (GDPR Art. 6(1)(b), contract).
- Security/fraud prevention (GDPR Art. 6(1)(f), legitimate interests).
- Legal obligations (GDPR Art. 6(1)(c), if applicable).

## 4. Recipients and processors
- Hosting: DigitalOcean.
- Backup storage:
- Monitoring/logging:

## 5. International transfers
- Specify where data is stored and what safeguards are used (SCCs if relevant).

## 6. Retention
- Active account data: until account deletion.
- Backups: [X days].
- Security logs: [X days].

## 7. Data subject rights
- Access/export, rectification, erasure, restriction, objection, complaint.
- In-app endpoints:
  - `GET /api/account/export`
  - `DELETE /api/account/`

## 8. Security measures
- Encrypted transport (HTTPS).
- Password hashing.
- Auth rate limiting + lockout.
- Least-privilege access to infrastructure.

## 9. Breach process
- Internal response and notification process (target within 72h when required).

