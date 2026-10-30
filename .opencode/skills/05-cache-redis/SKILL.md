---
name: 05-cache-redis
description: Solo blacklist/2FA IMemory, 0 caché entidades (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# 05-cache-redis MVP (lite)

## Incluye
- `blacklist:{jti}` TTL = vida remanente + `attempts:` 2FA TTL 60-120s (`nv-07`) + nunca password/secret/token. Lockout en DB (`SDDSegBloqueo`), no en caché.

## Excluye
- `cache:{entidad}`, write-through, fallback Redis, `CacheFallbackTests` entidades.

## Verify
- Solo keys IMemory 2FA/blacklist; 0 `CacheService` entidades.
