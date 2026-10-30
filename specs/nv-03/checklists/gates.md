# Checklist — nv-03 Puerta de Calidad Incremental (advisory)

> Trazabilidad: `spec §1-§7 ↔ plan §1-§5 ↔ task T1`. Estado: ✅ Aprobada y firmada.

- [x] `specs/nv-03/spec.md` existe con 7 secciones + Aprobación (plantilla canónica)
- [x] Secuencia 6 capas explícita (Security → cobertura base → Unit&Integration → Database → Contract → Mutation advisory)
- [x] Umbral 80% delta declarado como informativo (`--advisory`); bloqueo real sigue 45% global (§7)
- [x] Higiene declarada: Migraciones EF, DTOs anémicos `get; set;`, arranque (`Program.cs`) excluidos
- [x] Estrategia incremental: `since.target=main`, `mutate: [!**/Migrations/*]`, `thresholds.break=0`, test-projects acotado
- [x] Clerk limitado a reportar drift (`edit:deny`); aprobación asignada a revisor humano
- [x] Orden CI canónico intacto (puerta tras `coverage-merge` con `continue-on-error`); `endpoints.md` intacto; `CONTRACT_CAPTURE` vigente
- [x] `sdd-lint SDD-OK + critic PASS + endpoints OK + build 0/0` + 59/59 tests + piloto (delta 96.43%, mutación 45.83%)
- [x] Aprobación firmada: Usuario (aceptación expresa en sesión), 2026-10-11
