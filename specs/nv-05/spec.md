# NV-05 — Login + JWT sobre SDDUsuario

> Deriva de: `.specify/memory/constitution.md §2-§8` + `AGENTS.md §4-§6` + skills `03-auth-endpoints, 04-auth-matrix, 04-jwt-refresh, 04-security-core`.
> Predecesora: `nv-04` (CRUD SDDUsuario, Borrador). No la reescribe; exige las mejoras M1/M2/M4 de §7. Reutiliza hasher Argon2id de nv-04 y JWT ya cableado en `Hosting/ServiceCollectionExtensions.cs`.

## 1. Objetivo

Emitir JWT HS256 real tras validar `strNombreUsuario + strPWD` contra el hash Argon2id de `SDDUsuario`, con anti-enumeración, lockout DB y rate Login, sin revelar si falló el usuario o la contraseña.

## 2. Alcance / No-alcance

In: `POST /api/v1/auth/login [AllowAnonymous]` + emisión JWT (`sub/jti/role`) + `AdminPolicy` (madre auth, la consume `nv-04`) + `SDDSegBloqueo` + mejoras M1/M2/M4 a `nv-04` + tests + 1 fila en `docs/endpoints.md` + tag `v1.0.0-auth5`.
Out: `POST /api/v1/auth/logout` en `nv-06`; `/auth/refresh`, `/two-factor/*` en `nv-07`; este slice deja `jti`+blacklist listos. Saga/Redis/OTel/NBomber/Chaos/Pact-real/hosting/CD, `CacheService` entidades, cambios de pipeline.

## 3. Contratos API

Base `api/v1/auth`, JSON legacy `PropertyNamingPolicy=null`. Detalle vivo en `docs/endpoints.md`; aquí solo el contrato mínimo.

| Método | Ruta | Auth | Rate | Códigos |
|---|---|---|---|---|
| POST | `/api/v1/auth/login` | no (`AllowAnonymous`) | Login + Global | 200/400/401/422/429/500 |

Request `LoginRequestDto{strNombreUsuario, strPWD}` (plano 8–10, nunca se persiste ni se loguea). Response `200 LoginResponseDto{strToken, dteExpira}` (tiempos desde `TimeProvider`, expiración corta medida empíricamente). Error de credenciales → `401` idéntico `invalid_credentials` exista o no el usuario; validación de forma → `422`; rate → `429` con `Retry-After`; `500` vía middleware global.

## 4. Modelo de datos

Sin tabla nueva de usuarios. Reutiliza `SDDUsuario` + 2 ajustes (migraciones sin `_`, `CA1707`):

- M1: `SDDUsuario.strRol nvarchar(20) NOT NULL DEFAULT 'Usuario'` (`Admin|Usuario`).
- M2: tabla `SDDSegBloqueo` (`ToTable("SDDSegBloqueo")`, `HasKey(strNombreUsuario)`): `{strNombreUsuario nvarchar(10) PK/único, intIntentos int NOT NULL, dteBloqueoHasta datetime2 NULL, RowVersion rowversion NOT NULL}`.

`sub` del token = `SDDUsuario.id`; `role` = `strRol`.

## 5. Seguridad

- Frontera: `LoginRequestValidator` espejo (usuario `^[a-zA-Z0-9]{8,10}$`, pwd 8–10 requerido; `Trim` usuario, sin normalizar mayúsculas para buscar como en nv-04). `422` vía middleware.
- Verificación: buscar por `strNombreUsuario`; si no existe, ejecutar `Verify` contra fake-hash y retornar `401` idéntico (anti-enumeración + tiempo constante). Si existe: si `dteBloqueoHasta > UtcNow` → `401` idéntico (no revelar bloqueo); si `Verify==false` → incrementar `intIntentos`, a los 5 fijar `dteBloqueoHasta = UtcNow + 15min`, `401` idéntico; si `Verify==true` y bloqueado → `401`; si ok → resetear bloqueo, `NeedsRehash → re-hash` y emitir JWT. Argon2id es unidireccional: se verifica, no se desencripta.
- JWT: `key ≥32B solo env, Issuer/Audience, HS256, ClockSkew=Zero, claims sub=id/jti=guid/role=strRol`, expiración corta. `OnTokenValidated` anti-blacklist existente intacto. `strPWD`/token nunca en logs (filtro Serilog) ni en caché (prohibido `cache:{entidad}`).
- Pipeline, headers, CORS 1 origen, Kestrel/60s intactos. Sin secretos en repo (`critic B1`); inglés en código.

## 6. Tests / Gates

- Unit: validator (formas válidas/inválidas), servicio con hasher mock (no-existe vs pwd-mal vs ok vs bloqueado → siempre `401` idéntico salvo ok), `NeedsRehash`, mapeo claims `sub/jti/role`.
- Integration (WAF + Testcontainers puerto fijo): registro nv-04 → `login 200 + JWT decodificable HS256` → `login pwd-mal 401` x5 → `6.º 401` bloqueado → `401` idéntico con usuario inexistente → `422` forma mala → `401` sin token en ruta Admin (regresión nv-04).
- Security: plano/token ausentes en logs, rate `Login` fuerza `429`, `AdminPolicy` deniega sin rol, `ClockSkew=Zero` y `ValidAlgorithms=[HS256]` verificados.
- Database: `SDDSegBloqueo` persiste intentos/bloqueo; reintento concurrencia fail-closed (cerrado = denegar).
- Contract (`CONTRACT_CAPTURE=1`): `IsConventional` legacy (`strNombreUsuario/strPWD/strToken`), sin PascalCase puro.
- Gates `AGENTS.md §4`: `build 0/0 → 5 suites --no-build 100% → critic PASS → endpoints OK → sdd-lint → coverage ≥45% → delta advisory → stryker advisory → semgrep 0 → curl /health/ready 200 + login humo`.

## 7. Plan de entrega

`restore → build → tests → gates → publish` (sin desplegar) + `curl /health/ready + login 200 + login-mal 401 + ruta Admin 401/403 + headers` + `Memoria.md` + `CHECKLIST_PR.md` + 1 revisor + tag `v1.0.0-auth5`. `Borrador con evidencia` hasta firma.

## Aprobación

- [x] Revisor humano — Usuario (aceptación con merge PR #3)
- [x] Fecha: 2026-10-12
- [x] CHECKLIST_PR.md OK
- [x] Sin conflicto con constitución
- **Estado:** ✅ Aprobada
