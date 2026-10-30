# plan.md — NV-04 SDDUsuario

> Deriva de `specs/nv-04/spec.md`. Respeta constitución §4 (pipeline fijo) y §3 (sin librerías nuevas sin evaluar).

## 1. Contexto

Spec origen: `specs/nv-04/spec.md`. Primera entidad real (`AppDbContext` hoy vacío válido). Reutiliza molde `nv-02` (`PagedResult`, `PaginationQuery`, registro por reflexión) y puerta advisory `nv-03`. Requiere evaluar la librería Argon2id antes de referenciarla (constitución §3: licencia/mantenimiento/seguridad).

## 2. Diseño técnico

- **Entidad** `Data/Entities/SDDUsuario.cs` (internal): `id:int, strNombreUsuario:string(10), strPWD:string(200) hash, strCorreoElectronico:string(50), strRol:string(20) 'Usuario' (M1 nv-05), dteFechaRegistro:DateTime?, RowVersion:byte[] [Timestamp]`.
- **Fluent API** en `AppDbContext.OnModelCreating`: `ToTable("SDDUsuario")`, `HasKey(id)`, `strNombreUsuario HasMaxLength(10) IsRequired + HasIndex IsUnique`, `strCorreoElectronico HasMaxLength(50) IsRequired + HasIndex IsUnique`, `strRol HasMaxLength(20) IsRequired + HasDefaultValue("Usuario")`, `strPWD HasMaxLength(200) IsRequired`, `RowVersion IsRowVersion()`. Migración `AddInitialSDDUsuario` (sin `_`, `CA1707`).
- **DTOs** `Dtos/SDDUsuarioDtos.cs` (cero `strPWD` en lectura): `SDDUsuarioCreateDto{strNombreUsuario,strPWD,strCorreoElectronico,strRol? solo-Admin}`, `SDDUsuarioUpdateDto{strNombreUsuario?,strPWD?,strCorreoElectronico?,strRol? solo-Admin,RowVersion base64 required}`, `SDDUsuarioReadDto{id,strNombreUsuario,strCorreoElectronico,strRol,dteFechaRegistro,RowVersion base64}`; lista = `PagedResult<SDDUsuarioReadDto>`. `PropertyNamingPolicy=null` preserva wire legacy.
- **Validators** espejo auto-registrados: `SDDUsuarioCreateValidator` (usuario regex `^[a-zA-Z0-9]{8,10}$`, pwd 8–10 + 4 clases, correo + máx 50, `strRol` en `Admin|Usuario` si se envía), `SDDUsuarioUpdateValidator` (al menos 1 campo mutable + `RowVersion` base64 obligatoria). El servicio re-verifica duplicados → `Conflict` (`409`) y restringe `strRol` a Admin (`403` si no autorizado). Constantes de password compartidas en `Security/PasswordPolicy.cs` con `nv-05`.
- **Servicio** `Services/ISDDUsuarioService.cs + SDDUsuarioService.cs` (scoped, sin `new` clave, sin `IMemoryCache` de entidades): `CreateAsync` (normaliza `Trim/Lower` correo, verifica únicos, `HashPassword`, fija `UtcNow`, retorna `Read` con `RowVersion` fresca), `UpdateAsync` (carga por id `404`, fija `OriginalValue RowVersion`, aplica patch, re-hash si hay pwd, `409` en `DbUpdateConcurrencyException`), `ListAsync(PaginationQuery)` (vía `PaginationQueryValidator`, `Count + Skip/Take`), `GetByIdAsync`, `DeleteAsync(id, rowVersionBytes)` (`404/409`, borrado físico por id).
- **Hash** `Security/Argon2idPasswordHasher.cs` (`IPasswordHasher`): `Hash(plano 8–10) → PHC ≤200`, `Verify(hash, plano)`, `NeedsRehash(hash)` (re-hash transparente exigido por `nv-05`). Parámetros `64MB/3iter`, paralelismo medido empíricamente.
- **Controller** `Controllers/SDDUsuariosController.cs`: `[ApiController][ApiVersion("1.0")][Route("api/v{version:apiVersion}/[controller]")][Authorize(Policy="AdminPolicy")]`, sin `try/catch`; `POST 201 CreatedAtAction`, `PUT 200`, `GET 200`, `DELETE 204`; `DELETE rowVersion` vía `FromQuery string rowVersion → Convert.FromBase64String` (`400` si falla); `422` lo emite el middleware.
- **DI**: `AddScoped<ISDDUsuarioService, SDDUsuarioService> + AddSingleton<IPasswordHasher, ...>` sin reordenar `AddJwt/RateLimiter/Cors/Hsts`; validators ya cubiertos por `ValidatorRegistration` de `nv-02`.

## 3. Pipeline orden

Sin cambios en `Program.cs:44-70`: `SecurityHeaders outermost → CorrelationId → Serilog → ExceptionHandling → RequestTimeouts → ForwardedHeaders → HSTS no-Dev → HttpsRedirection (solo Prod) → CORS 1 origen → RateLimiter antes Auth → Auth → AuthZ → Controllers → /health*`. `critic B3` lo verifica por índice.

## 4. Riesgos / Deuda

- Nueva dependencia KDF sin evaluar → bloquear PR hasta ficha licencia/mantenimiento/seguridad (§3).
- Unicidad case-insensitive depende de collation SQL; mitigación: verificación aplicativa + índice único + test `USUARIO01` vs `usuario01`.
- `dteFechaRegistro` nula en DB admite nulos legacy; mitigación: el servicio siempre la fija + test de inmutabilidad.
- `IMemoryCache` 1-instancia (este slice no la usa; blacklist con TTL remanente llega en `nv-06`); sin Redis, documentado en `Memoria.md`.
- `DELETE ?rowVersion=` expone base64 en URL; mitigación: `SecretScrubbingEnricher` + alternativa `If-Match` anotada para revisión del revisor (el revisor elige antes de T3; el ContractTest fija la elección).

## 5. Verificación

`dotnet build -c Release --no-restore 0/0 → test Unit/Integration/Security/Database/Contract -c Release --no-build 100% → critic PASS (B1-B4: 5 filas nuevas en endpoints.md) → check_endpoints OK → sdd-lint SDD-OK → check_coverage ≥45% → delta advisory --min 80 → stryker since:main advisory + build posterior → semgrep 0 nuevos → publish + curl /health/ready 200`.
