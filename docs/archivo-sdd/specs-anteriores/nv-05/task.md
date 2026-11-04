# NV-05 — Tasks Login + JWT

> Slice liberable auth-1: login → JWT → AdminPolicy. Singular intencional (canónico plural en `.specify/templates/tasks-template.md`).

## T1 — M1/M2 sobre nv-04 + DbSets

- **Crear**: migración `AddRolSDDUsuario` (`strRol`), entidad + migración `AddSegBloqueo` (`SDDSegBloqueo`), `DbSet` + Fluent API.
- **Verificar**: `DatabaseTest` aplica ambas en contenedor; `build 0/0`.
- **No hacer**: emisión JWT aún.

## T2 — DTOs + validator + servicios auth

- **Crear**: `Dtos/LoginDtos.cs`, `Validators/LoginRequestValidator.cs`, `Services/IAuthService.cs + AuthService.cs` (fake-hash, lockout 5→15min `TimeProvider` fail-closed, re-hash), `Security/IJwtTokenService.cs + JwtTokenService.cs` + `AdminPolicy`.
- **Verificar**: `UnitTest` (4 casos `401` idéntico + ok + re-hash + claims) + ficha tiempos en `Memoria.md`.
- **No hacer**: `refresh/logout/2FA`, loguear plano/token, `cache:{entidad}`.

## T3 — Controller + docs vivos

- **Crear**: `Controllers/AuthController.cs` (`POST login` anónimo + rate `Login`, sin `try/catch`).
- **Modificar**: `docs/endpoints.md` (+1 fila con Spec=nv-05), `rate-limit-matrix.md` (cambiar Spec `Login` a `nv-05`), `asvs-l2-checklist.md` (V2/V5).
- **Verificar**: `Integration/Security/Contract` (ciclo 200→5x401→bloqueo→401-idéntico-inexistente→429→`CONTRACT_CAPTURE=1`).

## T4 — Gates + publish + cierre

- **Ejecutar**: `build → 5 suites --no-build → critic PASS → endpoints OK → sdd-lint → coverage ≥45% → delta/stryker advisory → semgrep → publish + curl /health/ready + humo login + headers`.
- **Cerrar**: `Memoria.md` (fake-hash, lockout, `sub=id`, M1/M2/M4 aplicadas a nv-04, M3 refresh reservada nv-07), `CHECKLIST_PR.md`, tag `v1.0.0-auth5` en Borrador hasta firma.
