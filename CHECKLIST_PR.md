# CHECKLIST_PR.md — Obligatorio

> Constitución §8. 1 revisor. `main` protegida.

- [x] Spec en `specs/NV-0X` aprobada y enlazada (sin cambio de contrato sin spec)
- [x] Build Release `dotnet build -c Release` sin errores CA3000+
- [x] Unit verdes
- [x] Integration SQL real (Testcontainers puerto fijo) verdes
- [x] Security + Database verdes
- [x] Contract captura `CONTRACT_CAPTURE=1` + `IsConventional` + `RowVersion AQ==`
- [x] Endpoints: `docs/endpoints.md` actualizado
- [x] Rate limits: `docs/rate-limit-matrix.md` verificado
- [x] ASVS: `docs/asvs-l2-checklist.md` verificado
- [x] Semgrep sin altos
- [x] Cobertura ≥45% (`python scripts/check_coverage.py`)
- [x] Headers + `/health/ready` + `401` verificados manualmente
- [x] `Memoria.md` actualizada, sin secretos
- [x] YAML válido (`yaml.safe_load`), `dotnet build` después de Stryker si se usó
- [x] Revisor: Usuario  Fecha: 2026-10-12
