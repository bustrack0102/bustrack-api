---
name: 04-jwt-refresh
description: JWT HS256 + rotación refresh
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 04-jwt-refresh MVP

## What I do
- `JwtBearer HS256 key≥32B solo env, ClockSkew=Zero, ValidAlgorithms=[HS256], Issuer/Audience, claims sub/jti/role/2fa_temp`.
- `OnTokenValidated` anti-blacklist. Refresh hash SHA-256 + rotación ReplacedBy + 7d + DbUpdateConcurrency→Invalid.
- Sin `System.IdentityModel` nuevo si se evita. Tokens nunca en logs.

## Verify
- `key≥32B` + blacklist `{jti}` + rotación probadas.
