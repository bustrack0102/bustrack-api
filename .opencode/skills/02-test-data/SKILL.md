---
name: 02-test-data
description: TestDataFactory + provider-states
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 02-test-data MVP

## What I do
- Builders deterministas + IDs seed. `POST /provider-states {state} → 200/400` gateado `EnableProviderStates+no-prod`.
- WAF nunca ve secretos de máquina (`UseLocalSettings` opt-in); suites con contenedor nunca en paralelo local (puerto fijo).
- Tests siempre `Testcontainers.MsSql` puerto fijo, nunca InMemory negocio.
- `SecurityTest` reuse `UnitTest/Common/TestDataFactory`.

## Verify
- Suites Integration/Security verdes con SQL real.
