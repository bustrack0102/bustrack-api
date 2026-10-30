---
name: 04-auth-matrix
description: Matriz endpoint↔auth/rate + ASVS lite
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 04-auth-matrix MVP

## What I do
- 5 policies `Login5/5min, Login2fa10/5min, Global1000/min, Admin200/min, Concurrent10`. `OnRejected 429 ErrorResponse + Retry-After`.
- Canónicos `rate-limit-matrix.md + asvs-l2-checklist.md`, specs solo enlazan. Test estructural `toda action con policy`.
- `UseRateLimiter` antes Auth. 429 uniforme.

## Verify
- Toda ruta con fila + toda action con policy (test estructural verde).
