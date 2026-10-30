# Checklist — nv-03.1 Blindaje de solución (slnx canónico)

> Trazabilidad: `spec §1-§7 ↔ plan §1-§5 ↔ task T1`. Estado: ✅ Aprobada y firmada (Usuario, 2026-10-12).

- [x] `specs/nv-03.1/spec.md` existe con 7 secciones + Aprobación (plantilla canónica)
- [x] `slnx` canónico declarado: 6 proyectos reales, sin `Chaos/Performance/MutationTest`
- [x] Gate `scripts/check_slnx.ps1` diseñado: parse `<Project Path>` + `Test-Path`, FAIL exit 1 con ruta faltante
- [x] Cableado dentro del step `critic` existente (sin job nuevo, orden CI intacto `B3`)
- [x] Deuda diferida ratificada: NBomber/Chaos/perf y mutation-gate fuera de alcance (dueño `nv-03` para mutación)
- [x] Clerk limitado a reportar drift (`edit:deny`); aprobación asignada a revisor humano
- [x] `endpoints.md` intacto; `stryker-config.json` intacto; specs firmadas intactas
- [x] `sdd-lint SDD-OK + critic PASS + slnx PASS + endpoints OK` + build slnx 0/0 + negativo verificado
- [x] Aprobación firmada: Revisor Usuario Fecha 2026-10-12
