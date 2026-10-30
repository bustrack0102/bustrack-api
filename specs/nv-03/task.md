# NV-03 — Puerta de Calidad Incremental (advisory)

## T1 — Borradores + diseños + implementación piloto, sin firmar
- **Crear**: `specs/nv-03/spec.md` (puerta 6 capas, advisory, higiene), `plan.md`, `task.md`, `checklists/gates.md` (casillas físicas), estado 🚧 Borrador.
- **Implementar (T2 ejecutado)**: `scripts/check_delta_coverage.py --min 80 --advisory` (delta `merge-base main` + worktree, unión por línea, higiene), `stryker-config.json` (`since.target=main`, `mutate` sin `Migrations`, `thresholds.break=0`), `ExcludeByFile` en runsettings, jobs `delta-check + stryker` en `ci-pr.yml` (`continue-on-error`, `fetch-depth: 0`).
- **Medir**: piloto slice nv-02 — delta 108/112 = 96.43% advisory OK; Stryker 5.0.0 Standard 23 mutantes en ~3min, score 45.83% (bloqueo al 80% hoy fallaría: advisory correcto); global 66.3% → 66.8% ≥45%.
- **Conciliar**: `Memoria.md` mapa (`nv-02`=molde/infraestructura, `nv-03`=puerta).
- **Verificar**: `sdd-lint SDD-OK + critic PASS + endpoints OK + build 0/0 + 59/59 tests`.
- **No hacer**: cambiar 45% o CI canónico, bloqueo por mutación (exige enmienda §7), firmar (solo revisor humano tras su revisión).
