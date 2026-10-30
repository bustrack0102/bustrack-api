---
name: drift-guards
description: Canónicos + extractores + CI anti-drift
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# drift-guards MVP

## What I do
- Canónicos `endpoints.md, rate-limit-matrix.md, asvs-l2-checklist.md`. `check_endpoints.ps1` bloqueante. No duplicar tablas en specs.
- Extractores truncan plantillas `{id}`/`{version}` igual en código y docs (si no, el diff bidireccional es imposible).
- `traceability-clerk` reporta drift, no edita.

## Verify
- `scripts/check_endpoints.ps1` → OK en CI-PR.
