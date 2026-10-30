# plan.md — NV-03 Puerta de Calidad Incremental (advisory)

## 1. Mapeo
- Crear: `specs/nv-03/spec.md` (7+Aprobación), `plan.md`, `task.md` (T1 singular), `checklists/gates.md` (casillas físicas).
- Diseñar (no implementar aún): `stryker-config.json` (`since.enabled/target=main`, `mutate: [**/*.cs, !**/Migrations/*, !**/*.Designer.cs]`, `test-projects: [UnitTest]`, `thresholds: {high:80, low:60, break:0}` advisory, `reporters: [cleartext, json]`) y `scripts/check_delta_coverage.py` (`--min 80 --advisory`, delta vía `merge-base`, lectura de `coverage.cobertura.xml` por archivo).
- Modificar (ejecutado T2): `coverlet.runsettings` (`ExcludeByFile: Migrations/*, Program.cs, Dtos/*`), `ci-pr.yml` (+2 steps tras `coverage-merge`, `fetch-depth: 0`), `Memoria.md` (mapa: `nv-02`=molde, `nv-03`=puerta).
- No tocar: `API_SDD/Program.cs` orden, `Controllers/`, `AppDbContext`, `docs/endpoints.md`, umbral 45%.

## 2. Guardarraíles
- `B1` sin secretos/sin pendientes sin issue/sin `_` migraciones + `All + TreatWarningsAsErrors + NuGetAudit + Sonar` intactos.
- `B2` sin `token` en logs. `B3` pipeline y orden CI intactos (puerta = steps añadidos al final). `B4` sin controller nuevo = `endpoints.md` intacto.
- `sdd-lint`: cada spec con `## Aprobación` + `plan/task/checklists`; sin rutas `Api/` legacy (solo `API_SDD/`).
- `@traceability-clerk`: solo reporta drift (`edit:deny`, AGENTS §11); la aprobación es del revisor humano.

## 3. Pruebas
- Esta fase (docs): `sdd-lint SDD-OK` + `critic PASS` + `endpoints OK` (los 4 `.md` no alteran gates).
- Fase siguiente (implementación): piloto en 1 slice — `delta-check` informativo + `stryker since:main` mide score real y duración CI; evidencia `coverage.txt` + `mutation-report.json`.

## 4. Secuencia
1. `spec.md` → 2. `plan.md` → 3. `task.md` → 4. `gates.md` → 5. `Memoria.md` (mapa) → 6. `sdd-lint + critic + endpoints` en verde.
7. (Ejecutado T2) script + config + `ExcludeByFile` + job CI + piloto (delta 96.43%, mutación 45.83% en ~3min) + global 66.8% ≥45%.

## 5. Verificación
- `powershell -File scripts/check_sdd.ps1 → SDD-OK`
- `powershell -File scripts/critic-guardrails.ps1 → PASS`
- `powershell -File scripts/check_endpoints.ps1 → OK`
- `dotnet build -c Release --no-restore → 0/0` (sin cambios de código, intacto)
