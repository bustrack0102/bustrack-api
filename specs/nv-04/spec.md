# NV-04 — CRUD SDDUsuario

> Deriva de: `.specify/memory/constitution.md §3-§8` + `AGENTS.md §4-§6` + skills `03-api-vertical-slice, 04-auth-matrix, 04-security-core`.
> Predecesoras: `nv-01, nv-02, nv-03` aprobadas y firmadas. No las modifica. Reutiliza molde `nv-02` (`PagedResult`, registro de validators por reflexión) y puerta advisory `nv-03`.

## 1. Objetivo

CRUD liberable de `SDDUsuario`: registro, actualización, listado paginado, obtención por id y borrado por id, con concurrencia optimista (`RowVersion`) y hash Argon2id real desde el día 1. Los DTOs exponen solo la información requerida por prácticas DevSecOps (nunca el hash ni el plano).

## 2. Alcance / No-alcance

In: entidad `SDDUsuario` + migración + `DbSet` + DTOs + validators espejo + `ISDDUsuarioService` + `SDDUsuariosController` v1.0 + hasher Argon2id + 5 suites de tests + 5 filas en `docs/endpoints.md` + `Memoria.md` + tag `v1.0.0-crud4`.
Out: login/refresh/logout/2FA y emisión JWT (sub-slices `nv-05…nv-07`); enmiendas reservadas M1 (`strRol`) / M2 (`SDDSegBloqueo`) aplicadas en `nv-05 T1`; saga/Redis/OTel/NBomber/Chaos/Pact-real/hosting/CD; `CacheService` de entidades; cambios de pipeline o de `nv-01…nv-03`.

## 3. Contratos API

Base `api/v1/sddusuarios`, JSON legacy `PropertyNamingPolicy=null` (`str* + dte* + id + RowVersion`). Detalle vivo en `docs/endpoints.md`; este spec solo enlaza (trazabilidad, sin duplicar).

| Método | Ruta | Auth | Rate | Códigos |
|---|---|---|---|---|
| POST | `/api/v1/sddusuarios` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/400/401/403/409/422/429 |
| PUT | `/api/v1/sddusuarios/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 200/400/401/403/404/409/422/429 |
| GET | `/api/v1/sddusuarios?page=&pageSize=` | JWT AdminPolicy | Admin | 200/401/403/422/429 |
| GET | `/api/v1/sddusuarios/{id}` | JWT AdminPolicy | Admin | 200/401/403/404/429 |
| DELETE | `/api/v1/sddusuarios/{id}?rowVersion={base64}` | JWT AdminPolicy | Admin + ConcurrentWrites | 204/400/401/403/404/409/429 |

Lista retorna `PagedResult<SDDUsuarioReadDto>` (`nv-02`). `RowVersion` viaja en wire como `base64 string`. `DELETE` la recibe por query `rowVersion` (`400` si malformada; alternativa `If-Match` anotada como deuda en plan).

## 4. Modelo de datos

Tabla `SDDUsuario` (prefijo `SDD` + convención `str/dte + RowVersion`, constitución §5):

- `id int IDENTITY(1,1) PK NOT NULL`.
- `strNombreUsuario nvarchar(10) NOT NULL`, índice único `IX_SDDUsuario_strNombreUsuario`. Regla: `^[a-zA-Z0-9]{8,10}$` (8–10, solo letras y dígitos). Se aplica `Trim()` sin alterar mayúsculas; unicidad case-insensitive según collation + verificación aplicativa → `409` en duplicado.
- `strPWD nvarchar(200) NOT NULL`: guarda **solo hash PHC Argon2id 64MB/3iter**. El plano solo existe en frontera: longitud `8..10`, exige minúscula + mayúscula + dígito + especial `[^a-zA-Z0-9]`. Nunca se retorna ni se registra.
- `strCorreoElectronico nvarchar(50) NOT NULL`, índice único. Regla: formato correo estándar (`EmailAddress`) + `Trim().ToLowerInvariant()` + máx 50. Duplicado → `409`.
- `strRol nvarchar(20) NOT NULL DEFAULT 'Usuario'` (`Admin|Usuario`), solo modificable por Admin; se emite como claim `role` (enmienda M1 definida en `nv-05`, aplicada en `nv-05 T1`).
- `dteFechaRegistro datetime2 NULL` en DB por compatibilidad; el servicio la fija a `UtcNow` en `Create` y la trata como inmutable (lo enviado en `Update` se ignora).
- `RowVersion rowversion NOT NULL` (`byte[]` + `[Timestamp]` en EF): concurrencia optimista. Flujo completo: se retorna en Create/Read y se exige en Update/Delete; `DbUpdateConcurrencyException → 409`.

Migración EF sin `_` (`CA1707`). `AppDbContext` añade solo `DbSet<SDDUsuario>` + `HasIndex IsUnique x2` + `IsRowVersion()`.

## 5. Seguridad

- Frontera: DTOs validados con FluentValidation espejo auto-registrado (`nv-02`); `id > 0`; `rowVersion` base64 válida. Errores de validación → `422` vía `ExceptionHandlingMiddleware` existente.
- Hash: Argon2id real desde CRUD1 (prohibido SHA256 directo / BCrypt para crear; BCrypt solo `Verify $2a$/$2b$` de migración). `strPWD` jamás en caché (este slice no usa `IMemoryCache`; la blacklist `blacklist:{jti}` llega con `nv-05/nv-06`), logs (filtro Serilog) o respuestas.
- Auth: los 5 endpoints exigen `Bearer JWT HS256` (`ClockSkew=Zero`, `ValidAlgorithms=[HS256]`, `OnTokenValidated` anti-blacklist) + `AdminPolicy` (`401` sin token, `403` sin rol). Sin `AllowAnonymous`.
- Pipeline, headers (`nosniff/DENY/CSP nonce/HSTS 365d no-Dev/CORS 1 origen`), `Kestrel 10MB/2min`, timeout 60s: intactos, sin reorden (`Program.cs:44-70`).
- Sin secretos en repo (`critic B1`); inglés en código; sin pendientes sin issue (`S1135`).

## 6. Tests / Gates

- Unit (xUnit+FA+Moq): validators (usuario corto/largo/con símbolo, pwd sin especial o fuera de 8–10, correo inválido), mapeo `RowVersion byte[]<->base64`, servicio (fija `UtcNow`, inmutabilidad, duplicado → `Conflict`), hasher mock.
- Integration (WAF + `Testcontainers.MsSql` puerto fijo): `201 → GET 200 → PUT 200 con RowVersion → PUT rancio 409 → DELETE con RowVersion 204 → GET 404`; duplicado usuario/correo `409`; `401` sin token, `403` sin rol Admin.
- Security (WAF): hash nunca en `GET`, plano nunca en logs, rate `ConcurrentWrites`/`Admin` → `429` uniforme, `RowVersion` malformada → `400`.
- Database: migración aplica en contenedor efímero, unicidad a nivel DB, `rowversion` cambia tras `PUT`.
- Contract (`CONTRACT_CAPTURE=1` regenera): `IsConventional` legacy (sin PascalCase puro), `RowVersion AQ==` base64, `PagedResult{Items,TotalCount,Page,PageSize}`.
- Gates `AGENTS.md §4`: `build Release 0/0 → 5 suites --no-build 100% → critic PASS → endpoints OK → sdd-lint → coverage ≥45% → delta advisory 80% → stryker advisory (build posterior) → semgrep 0 nuevos → curl /health/ready 200`.

## 7. Plan de entrega

`dotnet restore → build → tests → gates → dotnet publish -c Release -o ./publish` (sin desplegar) + `curl /health/ready + humo CRUD + headers` + `Memoria.md` + `CHECKLIST_PR.md` + 1 revisor + tag `v1.0.0-crud4`. Estado `Borrador con evidencia` hasta firma.

## Aprobación

- [x] Revisor humano — Usuario (aceptación expresa en sesión)
- [x] Fecha: 2026-10-12
- [x] CHECKLIST_PR.md OK
- [x] Sin conflicto con constitución
- **Estado:** ✅ Aprobada
