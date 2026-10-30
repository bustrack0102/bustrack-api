# NV-06 — Tasks Logout

> Slice liberable auth-2: revocación access JWT. Singular intencional (canónico plural en `.specify/templates/tasks-template.md`).

## T1 — A1/A2 sobre nv-05 + servicio revocación

- **Crear**: `Security/ITokenRevocationService.cs + TokenRevocationService.cs` (prefijo `blacklist:{jti}`, TTL = `exp-now`, `TimeProvider`); garantizar `jti`+`exp` en `JwtTokenService` (A1).
- **Verificar**: `UnitTest` (revoca/TTL/sin-jti/expirado) + `build 0/0`.
- **No hacer**: tablas, refresh, 2FA.

## T2 — Acción logout + DI

- **Crear**: `POST logout` en `Controllers/AuthController.cs` (`[Authorize]`, rate `Global`, → `204`, sin `try/catch`).
- **No tocar**: `rate-limit-matrix.md` (Global ya existe; prohibido crear política `Logout`).
- **Modificar**: DI (`AddSingleton` revocación).
- **Verificar**: `IntegrationTest` (cadena 200→204→401→401-repetido→401-sin-token).
- **No hacer**: loguear token, `cache:{entidad}`, política rate nueva.

## T3 — Docs vivos + regresión

- **Modificar**: `docs/endpoints.md` (+1 fila con Spec=nv-06), `asvs-l2-checklist.md` (V2 Auth línea blacklist, no V2 2FA).
- **Verificar**: `SecurityTest` (no-token-en-logs, `429`, manipulado `401`) + regresión nv-04 (revocado → `401` en Admin) + `ContractTest CONTRACT_CAPTURE=1` (`204` vacío).

## T4 — Gates + publish + cierre

- **Ejecutar**: `build → 5 suites --no-build → critic PASS → endpoints OK → sdd-lint → coverage ≥45% → delta/stryker advisory → semgrep → publish + humo + headers`.
- **Cerrar**: `Memoria.md` (TTL remanente vs 120s, single-instance, 401-en-doble-logout, refresh reservado nv-07), `CHECKLIST_PR.md`, tag `v1.0.0-auth6` en Borrador hasta firma.
