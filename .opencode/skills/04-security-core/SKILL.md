---
name: 04-security-core
description: Argon2id + lockout + headers
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 04-security-core MVP

## What I do
- `Argon2id 64MB/3iter PHC $argon2id$, BCrypt solo Verify, NeedsRehash capacidad`.
- KDF evaluada §3: `Konscious 1.3.1 MIT` elegida, `Isopoh 2.0.0` rechazada (licencia CC-BY-4.0 sin grant de patentes).
- `MapInboundClaims=false` en JwtBearer para preservar `sub/jti/role` (el mapeo WS-Fed borra `role` → 403 fantasma).
- `SegBloqueo strNombre único + TimeProvider + 1 reintento DbUpdateConcurrency fail-closed`.
- `SecurityHeaders outermost + CSP nonce 16B + HSTS365d MaxAge solo no-Dev`. Password nunca en caché/logs.

## Verify
- Hash real + lockout DB + headers (nosniff, DENY, CORS 1 origen) probados.
