# NV-03 — Puerta de Calidad Incremental (cobertura delta + Stryker advisory)

> Deriva de: `.specify/memory/constitution.md §7-§8` + `AGENTS.md §4` + `skills 10-testing-strategy/testing-pact/testing-mutation`.
> Predecesoras: `nv-01` y `nv-02` aprobadas y firmadas 2026-10-11. No las modifica. `nv-02` queda reservado al molde/infraestructura.

## 1. Objetivo
Automatizar por cada CRUD/funcionalidad una puerta de calidad incremental: medir cobertura del **delta** (mínimo 80% **informativo**) y ejecutar Stryker focalizado en lo cambiado (**advisory**), integrados con la suite obligatoria y sin alterar el orden CI canónico.

## 2. Alcance / No-alcance
In: secuencia de 6 capas en modo advisory + diseño de `stryker-config.json` + diseño de `scripts/check_delta_coverage.py --min 80 --advisory` + exclusiones de higiene + checklist de instanciación.
Out: bloqueo por 80% delta o mutation score (requiere enmienda §7; hoy el único bloqueo es 45% global §7); reorden del pipeline (AGENTS §3); evaluaciones Stryker globales en flujo ordinario; NBomber/Chaos/Pact-real/perf; saga/Redis/AWS/OTel/hosting.

## 3. Contratos API
Sin cambio de contrato. `docs/endpoints.md` intacto. Los CRUDs futuros enlazan su spec; esta puerta solo añade gates, no rutas. Convención legacy y `PagedResult` según `nv-02`.

## 4. Modelo de datos
Sin modelo. Sin `DbSet`/entidad/migración en esta puerta. Regla `CA1707` sin `_` sigue vigente para futuros CRUDs.

## 5. Seguridad
Sin superficie nueva. La capa Security reutiliza lo existente: `NuGetAudit` en build + suite `SecurityTest` (WAF) + `semgrep` último + `critic B1/B2`. Sin secretos en Git (`B1`), sin `token` en logs salvo `blacklist` (`B2`). Pipeline intacto (`B3`), `endpoints.md` intacto (`B4`). Inglés en código.

## 6. Tests / Gates
Secuencia por funcionalidad (orden CI canónico `ci-pr.yml:18`, la puerta corre tras `coverage-merge`):
1. `Security` (suite WAF + NuGetAudit + semgrep, existentes).
2. Métricas base: `coverage-merge` → `check_coverage.py --min 45` (bloqueante, §7).
3. `Unit + Integration` SQL real (existentes).
4. `Database` contenedores efímeros + `rowversion` (existente).
5. `Contract` `CONTRACT_CAPTURE` + `IsConventional` (existente).
6. Puerta incremental (nueva, advisory): `check_delta_coverage.py --min 80 --advisory` (delta = `git merge-base main` → `*.cs` bajo `API_SDD/`, excluye higiene; informa, exit 0) + `dotnet stryker --config-file stryker-config.json` (`since.target=main`, `mutate: [!**/Migrations/*]`, `thresholds` sin break, registra score).
Higiene de métricas (excluidas del delta y de mutación): migraciones EF generadas, DTOs/modelos anémicos solo `get; set;`, arranque (`Program.cs`, cableado framework). Recalibrar empíricamente el 45% global tras aplicar exclusiones (§2.5).

## 7. Plan de entrega
Sin `publish`, sin tag, sin endpoint. Entrega: 4 documentos en Borrador + diseños de `stryker-config.json` y `check_delta_coverage.py`. Implementación (script + config + job CI + piloto medido en 1 slice) = fase siguiente con su `T2`. Tras Stryker siempre `dotnet build` antes de `--no-build`.

## Aprobación
- [x] Revisor humano — Usuario (aceptación expresa en sesión)
- [x] Fecha: 2026-10-11
- [x] CHECKLIST_PR.md OK
- [x] Sin conflicto con constitución
- **Estado:** ✅ Aprobada
