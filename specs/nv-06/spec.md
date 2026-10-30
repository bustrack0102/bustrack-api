# NV-06 — Logout (revocación JWT)

> Deriva de: `.specify/memory/constitution.md §2-§8` + `AGENTS.md §4-§6` + skills `03-auth-endpoints, 04-auth-matrix, 04-jwt-refresh`.
> Predecesoras: `nv-04` (CRUD) y `nv-05` (login, Borrador). No las reescribe; exige A1/A2 de §7. Reutiliza `OnTokenValidated` anti-blacklist ya cableado en `Hosting/ServiceCollectionExtensions.cs:165-182`.

## 1. Objetivo

Revocar el access JWT vigente (`blacklist:{jti}` en `IMemoryCache`) para que su reúso sea `401`, sin estado en SQL y sin abrir superficie nueva.

## 2. Alcance / No-alcance

In: `POST /api/v1/auth/logout [Authorize]` → `204` + blacklist con expiración = vida remanente + abstracción `ITokenRevocationService` + tests + 1 fila en `docs/endpoints.md` + tag `v1.0.0-auth6`.
Out: revocación de refresh (`nv-07`), 2FA, cambios a `SDDUsuario`/`SDDSegBloqueo`, nuevas políticas de rate, Redis, cambios de pipeline/hosting/CD.

## 3. Contratos API

Base `api/v1/auth`, sin cuerpo de entrada ni salida. Detalle vivo en `docs/endpoints.md`.

| Método | Ruta | Auth | Rate | Códigos |
|---|---|---|---|---|
| POST | `/api/v1/auth/logout` | JWT (cualquier rol, incl. `Usuario`; no exige `AdminPolicy`) | Global | 204/401/429 |

`204` = `jti` registrado. `401` = sin token, inválido, expirado o ya revocado (incluido el doble logout con el mismo token: el pipeline falla en `OnTokenValidated` antes del controller). `429` con `Retry-After`.

## 4. Modelo de datos

Sin tablas ni migraciones. Estado solo efímero: `IMemoryCache["blacklist:{jti}"] = expUnix`, `AbsoluteExpiration = exp - TimeProvider.UtcNow` (si expirado, no se registra). Clave idéntica a la que lee `OnTokenValidated`. Nunca se guarda el token.

## 5. Seguridad

- Extrae `jti` + `exp` del `ClaimsPrincipal` ya validado (no revalida firma; exige `jti` presente, si no → `401`).
- TTL = vida remanente (desviación documentada del "120s" de §2, que sigue aplicando a `attempts 2FA`; ver plan §4). Pérdida al reciclar = deuda 1-instancia ya amparada, sin Redis.
- Token/`jti` ajeno nunca en logs (filtro Serilog, `critic B2`); prohibido `cache:{entidad}`. Pipeline, headers, CORS 1 origen, Kestrel/60s intactos. Sin secretos en repo. Inglés en código.

## 6. Tests / Gates

- Unit: servicio con `IMemoryCache` fake + `TimeProvider` fijo (revoca → `IsRevoked true`; TTL ≈ `exp-now`; sin `jti` → `401`; expirado → no registra).
- Integration (WAF + Testcontainers): `login 200 → logout 204 → GET admin con mismo token 401 → logout repetido 401 → sin token 401`.
- Security: token ausente en logs, rate Global fuerza `429`, token manipulado sigue `401`, `ClockSkew=Zero` intacto.
- Database: sin cambios (aserción `NoPendingMigrations`: `Get-Migrations` sin delta).
- Contract (`CONTRACT_CAPTURE=1`): `204` sin cuerpo + fila legacy en `endpoints.md`.
- Gates `AGENTS.md §4`: `build 0/0 → 5 suites --no-build 100% → critic PASS → endpoints OK → sdd-lint → coverage ≥45% → delta/stryker advisory → semgrep 0 → curl /health/ready + cadena login/logout/reúso + headers`.

## 7. Plan de entrega

`restore → build → tests → gates → publish` (sin desplegar) + humo §6 + `Memoria.md` (decisión TTL + deuda single-instance) + `CHECKLIST_PR.md` + 1 revisor + tag `v1.0.0-auth6`. `Borrador con evidencia` hasta firma.

## Aprobación

- [x] Revisor humano — Usuario (aceptación con merge PR #5)
- [x] Fecha: 2026-10-12
- [x] CHECKLIST_PR.md OK
- [x] Sin conflicto con constitución
- **Estado:** ✅ Aprobada
