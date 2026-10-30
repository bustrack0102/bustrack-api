# NV-02 — Molde sin endpoints

## T1 — Base + DI + test, cero rutas
- **Crear**: `Common/PagedResult.cs {Items,TotalCount,Page,PageSize}`, `Dtos/PaginationQuery.cs`, `Services/IService.cs`, `Validators/AssemblyMarker.cs + ValidatorRegistration.cs + PaginationQueryValidator.cs`, `UnitTest/Architecture/VerticalSliceConventionTests.cs (5)`.
- **Modificar**: `Hosting/ServiceCollectionExtensions.cs` añade `AddValidatorsFromAssemblyContaining` (1 línea + using).
- **Verificar**: `build 0/0 + Unit 100% + critic PASS + endpoints OK + coverage ≥45% + semgrep 0`.
- **No hacer**: controllers, `DbSet`/entidades, migraciones, `AdminPolicy` usada, emisión JWT/TOTP/lockout, `cache:{entidad}`, filas `endpoints.md`, reorden pipeline, secretos, pendientes sin issue.
