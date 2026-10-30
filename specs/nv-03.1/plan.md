# plan.md — NV-03.1 Blindaje de solución (slnx canónico)

## 1. Mapeo
- Crear: `specs/nv-03.1/spec.md` (7+Aprobación, Borrador), `plan.md`, `task.md` (T1 singular), `checklists/gates.md` (casillas físicas).
- Implementar (T2 ejecutado): `scripts/check_slnx.ps1` (parse `<Project Path>` + `Test-Path` por entrada; FAIL exit 1 con la ruta faltante) + `ci-pr.yml` (invocación dentro del step existente de `critic`, sin job nuevo ni reorden) + `Memoria.md` (fase nv-03.1).
- No tocar: `API_SDD.slnx` (HEAD ya es el canónico 6 proyectos), `API_SDD/Program.cs` orden, `Controllers/`, `AppDbContext`, `docs/endpoints.md`, umbral 45%, `stryker-config.json` (`test-projects: [UnitTest]` intacto, dueño `nv-03`), specs `nv-00…nv-05` firmadas.

## 2. Guardarraíles
- `B1` sin secretos/sin pendientes sin issue/sin `_` migraciones + `All + TreatWarningsAsErrors + NuGetAudit` intactos.
- `B2` sin `token` en logs. `B3` pipeline y orden CI intactos (gate = 1 línea más dentro del step `critic`, no step nuevo). `B4` sin controller nuevo = `endpoints.md` intacto.
- `sdd-lint`: spec con `## Aprobación` + `plan/task/checklists`; sin rutas `Api/` legacy (solo `API_SDD/`).
- `@traceability-clerk`: solo reporta drift (`edit:deny`, AGENTS §11); la aprobación es del revisor humano.

## 3. Pruebas
- Esta fase (docs): `sdd-lint SDD-OK` + `critic PASS` + `endpoints OK` (los `.md` no alteran gates).
- T2 (implementación): positivo (`slnx` HEAD → `SLNX-OK` + build slnx 0/0) + negativo (copia temporal con ref fantasma `ChaosTest/ChaosTest.csproj` → `SLNX-FAIL` exit 1) + suites intactas (sin cambios de código).

## 4. Secuencia
1. `spec.md` → 2. `plan.md` → 3. `task.md` → 4. `checklists/gates.md` → 5. `check_slnx.ps1` → 6. cableado step `critic` → 7. `Memoria.md` (fase nv-03.1) → 8. positivo/negativo + `sdd-lint + critic + endpoints` en verde.
9. (Firma) solo revisor humano; este slice no se firma solo.

## 5. Verificación
- `powershell -File scripts/check_sdd.ps1 → SDD-OK`
- `powershell -File scripts/critic-guardrails.ps1 → PASS`
- `powershell -File scripts/check_slnx.ps1 → PASS` (y con ref fantasma `→ SLNX-FAIL` exit 1)
- `powershell -File scripts/check_endpoints.ps1 → OK`
- `dotnet build API_SDD.slnx -c Release --no-restore → 0/0` (sin cambios de código, intacto)
