# plan.md — NV-02 Molde sin endpoints

## 1. Mapeo
- Crear: `API_SDD/Common/PagedResult.cs`, `API_SDD/Dtos/PaginationQuery.cs`, `API_SDD/Services/IService.cs marker`, `API_SDD/Validators/AssemblyMarker.cs + ValidatorRegistration.cs + PaginationQueryValidator.cs`, `UnitTest/Architecture/VerticalSliceConventionTests.cs`.
- Modificar: `API_SDD/Hosting/ServiceCollectionExtensions.cs` solo añade `AddValidatorsFromAssemblyContaining<AssemblyMarker>` (sin reordenar, sin tocar `AddJwt/RateLimiter/Cors/Hsts`).
- No tocar: `API_SDD/Program.cs:44-70` orden, `Controllers/`, `Data/AppDbContext.cs`, `docs/endpoints.md`, migraciones.

## 2. Guardarraíles
- `B1` sin secretos/sin pendientes sin issue/sin `_` migraciones + `TreatWarningsAsErrors + All + NuGetAudit + Sonar` (`Directory.Build.props` intacto).
- `B2` sin `token` en logs. `B3` pipeline fijo verificado por índice de pasos. `B4` sin controller nuevo = `endpoints.md` intacto.
- Complejidad `≤10 método / ≤15 clase`, `.editorconfig CA3000+` error. `IOptionsMonitor` lazy preparado para futuros overrides WAF (no eager).

## 3. Pruebas
- `Unit/Architecture 5`: 1 PagedResult shape + 1 Services públicos/sin caché entidades + 1 Validators `AbstractValidator<>` + 1 sin `try/catch` en controllers + 1 DI resuelve validators.
- Manual: `grep DbSet/Controller/Migration` solo preexistentes `Ping/Probe`.

## 4. Secuencia
1. `PagedResult.cs + Marker + IService`.
2. `AddValidatorsFromAssembly` en DI.
3. `VerticalSliceConventionTests.cs`.
4. `build → unit → critic → endpoints → coverage → semgrep`.

## 5. Verificación
- `dotnet build -c Release --no-restore → 0/0`
- `dotnet test UnitTest -c Release --no-build → 100%`
- `powershell -File scripts/critic-guardrails.ps1 → PASS`
- `powershell -File scripts/check_endpoints.ps1 → OK`
- `python scripts/check_coverage.py --min 45`
- `semgrep scan --config=auto --config=.semgrep/semgrep.yaml --error → 0 nuevos`
