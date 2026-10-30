# NV-02 — Molde DTO→Validator→Service sin endpoints

> Deriva de: `.specify/memory/constitution.md §3-§4` + `AGENTS.md §5` + `skill 03-api-vertical-slice`.
> Predecesor: `nv-01` aprobada y firmada 2026-10-11. No la modifica.

## 1. Objetivo
Dejar el molde vertical cableado y verificable (`DTO→Validator espejo→Service público`), sin exponer ruta nueva, para que `nv-03+` (auth/CRUD) solo añadan entidad+controller.

## 2. Alcance / No-alcance
In: `Common/PagedResult.cs + Dtos/PaginationQuery.cs (DTO demo sin entidad) + Services/IService.cs marker + Validators/AssemblyMarker.cs + Validators/ValidatorRegistration.cs (registro por reflexión sin paquete DI extra, constitution §3) + Validators/PaginationQueryValidator.cs + 1 test arquitectura`.
Out: ningún `Controller`, ningún `DbSet`/entidad, ninguna migración, ninguna `AdminPolicy` usada, ninguna fila en `docs/endpoints.md`, ningún `CacheService` entidades, ningún `try/catch` en controller futuro, sin AWS/saga/Redis/OTel/NBomber/Chaos/Pact-real/hosting/CD.

## 3. Contratos API
Sin cambio de contrato. `docs/endpoints.md` queda intacto (10 filas `ping/probe/health/openapi/scalar` de `nv-01`). Solo se honra convención ya declarada en `docs/endpoints.md:20`: `PagedResult{Items,TotalCount,Page,PageSize}` + JSON legacy `PropertyNamingPolicy=null` + `api/v{version}/[controller]` v1.0 estricta.

## 4. Modelo de datos
Sin modelo. Sin `DbSet`, sin migración (regla `CA1707` sin `_` sigue vigente a futuro). `AppDbContext` sigue vacío válido como en `nv-01`. Tablas futuras: `Prefijo* + str/int/dec/dte/bln + RowVersion rowversion + decimal(18,2)`.

## 5. Seguridad
Sin superficie nueva. Se reutiliza: `ExceptionHandlingMiddleware: ValidationException→422 validation` existente, `FluentValidation 12.2.0` ya referenciado en `API_SDD.csproj`, `IMemoryCache` solo `blacklist:{jti}/attempts/lockout` TTL 120s, nada de `cache:{entidad}`. Sin secretos en Git (`critic B1`), sin `token` en logs salvo `blacklist` (`B2`). Pipeline `Program.cs` intacto en orden (`B3`). Inglés en código, sin pendientes sin issue (`S1135`).

## 6. Tests / Gates
`UnitTest/Architecture/VerticalSliceConventionTests.cs`: Services públicos sin `IMemoryCache cache:{entidad}`, Validators top-level no-anidados heredan `AbstractValidator<>`, sin `try/catch` en `*Controller.cs`, `PagedResult` con `Items/TotalCount/Page/PageSize` + wire JSON, `PaginationQueryValidator` válido `(1,20)` / inválido `(0,500)`, DI resuelve validators del assembly.
Gates: `build -c Release --no-restore 0/0` → `test Unit --no-build` 100% → `critic-guardrails.ps1 PASS (B1-B4)` → `check_endpoints.ps1 OK` → `coverage ≥45%` → `semgrep 0 nuevos`. `CONTRACT_CAPTURE=0` (sin entidad, como `nv-01`).

## 7. Plan de entrega
Sin `publish`, sin tag, sin `curl` nuevo. Solo `build+tests+gates` en vacío + `Memoria.md` al día. `B4` se mantiene porque no hay controller nuevo que exigir fila.

## Aprobación
- [x] Revisor 1 — Usuario (aceptación expresa en sesión)
- [x] Fecha: 2026-10-11
- [x] CHECKLIST_PR.md OK
- [x] Sin conflicto con constitución
- **Estado:** ✅ Aprobada
