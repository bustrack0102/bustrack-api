---
name: critic-guardrails
description: Gate pre-push 4 bloqueantes
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# critic-guardrails MVP

## What I do
- `B1 secretos + B2 token en logs + B3 pipeline alterado + B4 endpoints sin fila → FAIL`. `scripts/critic-guardrails.ps1 PASS exit 0`. PS5.1 hardening.
- Skips con backslash simple (el doble nunca excluye `bin/obj`); `appsettings.Local.json` sancionado exento en B1a/B1b, `.env/secrets.json` siguen prohibidos.
- En CI solo critic ligero bloqueantes 1-4. Sin Stryker aquí.

## Verify
- `scripts/critic-guardrails.ps1` → PASS.
