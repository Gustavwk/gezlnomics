# Security Incident Response Playbook

## 1. Triage (0-2h)
1. Confirm incident scope (service/data/user impact).
2. Isolate affected component if needed.
3. Preserve logs/evidence (do not rotate/delete).

## 2. Containment (same day)
1. Rotate compromised credentials.
2. Block malicious traffic patterns (Traefik/WAF/firewall/rate limits).
3. Patch or rollback the vulnerable release.

## 3. Assessment
1. Determine if personal data was affected.
2. Estimate impacted records and categories.
3. Decide reporting obligations with legal/privacy owner.

## 4. Notification
1. If required, notify authority within 72h of awareness.
2. Notify affected users when legally required.
3. Keep timeline and actions documented.

## 5. Recovery and follow-up
1. Verify system integrity and monitoring.
2. Run postmortem with concrete prevention actions.
3. Update runbooks, controls, and test cases.

