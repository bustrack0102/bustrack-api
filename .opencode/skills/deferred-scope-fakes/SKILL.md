---
name: deferred-scope-fakes
description: Real-first con NOTEs, sin fakes
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# deferred-scope-fakes MVP invertido

## What I do
- Prohibido `SHA256 fake / AnonymousChallenge / token opaco`. Solo `NOTE(real-first)` + `test-aserta-diferido: sin caché/SQS/OTel`.
- Cada slice usa `JWT real + Argon2id real` desde `nv-04/nv-05`; TOTP real en `nv-07`.

## Verify
- `grep` 0 fakes + tests asertan diferidos (sin CacheService/SQS/OTel).
