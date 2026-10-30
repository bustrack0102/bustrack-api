---
name: 03-auth-endpoints
description: Usuario + login + logout (nv-04…nv-06); refresh + 2FA en nv-07
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  fase: nv-04+
  opencode/slash: "true"
---
# 03-auth-endpoints MVP

> Fase `nv-04+` (auth; `nv-02`=molde, `nv-03`=puerta). No exigible en gates `nv-01` vacío (sin emisión).

## What I do
- `POST /usuarios + /auth/login + /auth/login2fa/verify + /auth/refresh [AllowAnonymous] + /auth/logout [Authorize] + /two-factor/* [Authorize]`.
- Anti-enumeración 401 idéntico + lockout DB 5→15min + blacklist `blacklist:{jti}` IMemory TTL remanente.
- 2FA TOTP `Otp.NET ±1 + DataProtection` en `nv-07`. `AdminPolicy` desde `nv-04` (madre auth).

## Verify
- Login 401 idéntico real + lockout fail-closed + refresh rotación.
