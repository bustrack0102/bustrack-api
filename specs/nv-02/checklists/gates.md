# Checklist — nv-02 Molde sin endpoints

> Trazabilidad: `spec §1-§7 ↔ plan §1-§5 ↔ task T1`. Estado: ✅ Aprobada y firmada.

- [x] `specs/nv-02/spec.md` existe con 7 secciones + Aprobación (plantilla canónica `.specify/templates/spec-template.md`)
- [x] Alcance solo arquitectura: sin controller, sin `DbSet`/entidad, sin migración, sin `AdminPolicy` usada
- [x] `Common/PagedResult.cs {Items,TotalCount,Page,PageSize}` + `Dtos/PaginationQuery.cs` + `Services/IService.cs` + `Validators/AssemblyMarker.cs + ValidatorRegistration.cs + PaginationQueryValidator.cs`
- [x] DI solo añade `AddValidatorsFromAssembly(AssemblyMarker.Value)` sin reordenar pipeline (`B3`)
- [x] `Unit/Architecture 5` verdes (35/35 Unit, 59/59 total con Integration 13 + Security 7 + Database 2 + Contract 2)
- [x] Gates: build 0/0 + critic PASS (B1-B4) + endpoints OK (10↔10) + coverage 66.3% ≥45% + semgrep 0 (stub local, real en CI)
- [x] `docs/endpoints.md` intacto (10 filas nv-01) + `CONTRACT_CAPTURE=0`
- [x] Aprobación firmada: Usuario (aceptación expresa en sesión), 2026-10-11
