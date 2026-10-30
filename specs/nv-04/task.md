# NV-04 — Tasks SDDUsuario

> Cada CRUD liberable: spec→migración→DTO→servicio→controller→tests→gates→tag. Nombre singular intencional (estándar plural en `.specify/templates/tasks-template.md`).

## T1 — Entidad + migración + DbSet

- **Crear**: `Data/Entities/SDDUsuario.cs`, migración `AddInitialSDDUsuario` (sin `_`), `DbSet<SDDUsuario>` + Fluent API (únicos x2 + `IsRowVersion`).
- **Verificar**: `DatabaseTest` aplica migración en contenedor efímero; unicidad a nivel DB; `build 0/0`.
- **No hacer**: controllers/DTOs aún.

## T2 — DTOs + validators espejo + hasher

- **Crear**: `Dtos/SDDUsuarioDtos.cs` (4 DTOs, cero `strPWD` en lectura), `Validators/SDDUsuarioCreateValidator.cs + SDDUsuarioUpdateValidator.cs`, `Security/Argon2idPasswordHasher.cs + IPasswordHasher.cs` (con `NeedsRehash`) + `Security/PasswordPolicy.cs` (constantes compartidas con nv-05).
- **Verificar**: `UnitTest` validators + `RowVersion byte[]<->base64` + hash/verify + ficha de la librería KDF en `Memoria.md`.
- **No hacer**: exponer hash, registrar plano, crear con BCrypt.

## T3 — Servicio + controller + docs vivos

- **Crear**: `Services/ISDDUsuarioService.cs + SDDUsuarioService.cs` (fija `UtcNow`, inmutable, `409` duplicado/concurrencia), `Controllers/SDDUsuariosController.cs` (5 acciones, `AdminPolicy`, sin `try/catch`).
- **Modificar**: DI (`AddScoped` servicio + `Singleton` hasher), `docs/endpoints.md` (+5 filas con Spec=nv-04), `rate-limit-matrix.md` (marcar `Admin/ConcurrentWrites` usadas).
- **Verificar**: `IntegrationTest` ciclo `201→200→409 rancio→204→404` + `401/403/429`; `ContractTest CONTRACT_CAPTURE=1` + `IsConventional`.

## T4 — Gates + publish + cierre

- **Ejecutar**: `build → 5 suites --no-build → critic PASS → endpoints OK → sdd-lint → coverage ≥45% → delta advisory → stryker advisory → semgrep → publish + curl /health/ready + humo CRUD + headers`.
- **Cerrar**: `Memoria.md` (decisiones: params Argon2id, `DELETE ?rowVersion`, `dteFechaRegistro` inmutable), `CHECKLIST_PR.md`, tag `v1.0.0-crud4` en Borrador hasta firma.
