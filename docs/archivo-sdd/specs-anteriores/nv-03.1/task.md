# NV-03.1 — Blindaje de solución (slnx canónico)

## T1 — Borradores + diseño, sin firmar
- **Crear**: `specs/nv-03.1/spec.md` (slnx 6 proyectos canónico + gate fail-ante-ref-fantasma + deuda diferida ratificada), `plan.md`, `task.md`, `checklists/gates.md` (casillas físicas), estado 🚧 Borrador.
- **Implementar (T2 ejecutado)**: `scripts/check_slnx.ps1` (parse `<Project Path>` + `Test-Path` por entrada; `SLNX-FAIL` exit 1 con la ruta faltante), cableado dentro del step `critic` de `ci-pr.yml` (sin job nuevo, orden intacto `B3`).
- **Medir**: positivo `slnx` HEAD (6/6 existen → `SLNX-OK`) + build slnx 0/0; negativo copia temporal con `ChaosTest/ChaosTest.csproj` (el diff revertido del 2026-10-11) → `SLNX-FAIL` exit 1; suites intactas (sin cambios de código).
- **Conciliar**: `Memoria.md` fase nv-03.1 (decisión: revert `slnx` correcto; 3 dirs inexistentes verificados; gate anti-regresión).
- **Verificar**: `sdd-lint SDD-OK + critic PASS + slnx PASS + endpoints OK`.
- **No hacer**: re-añadir `Chaos/Performance/MutationTest` (exige enmienda §7 + slices `nv-04+`); cambiar 45%, `stryker-config.json`, orden CI o specs firmadas; firmar (solo revisor humano tras su revisión).
