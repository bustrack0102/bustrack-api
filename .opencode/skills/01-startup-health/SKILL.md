---
name: 01-startup-health
description: /health + /ready con SQL externo + AssemblyIntegrity
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 01-startup-health MVP

## What I do
- `/health` 200 siempre + `/health/ready` 200 con SQL, 503 si cae.
- `AssemblyIntegrity` SHA-256 vs ExpectedSha256; Dev Degraded, Prod Unhealthy.
- Sin health-ui, sin /metrics full. JSON genérico sin trazas prod.

## Verify
- `curl /health/ready` 200 con DB externa, 503 si cae. `Detail` solo no-prod.
