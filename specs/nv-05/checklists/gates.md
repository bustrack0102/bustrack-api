# Checklist — nv-05 Login + JWT sobre SDDUsuario

> Trazabilidad: `spec §1-§7 ↔ plan ↔ task`. Estado: ✅ Aprobada y firmada por Usuario 2026-10-12 (merge PR #3).
> Archivo de higiene SDD (`check_sdd.ps1` B1 exige `checklists/` por spec). Contenido derivado de `spec.md §2`, sin ampliar alcance.

- [x] `POST /api/v1/auth/login [AllowAnonymous]` + emisión JWT HS256 real (`sub/jti/role`)
- [x] Anti-enumeración + lockout DB + rate Login; sin revelar usuario-vs-contraseña
- [x] `AdminPolicy` (madre auth) + `SDDSegBloqueo` + mejoras M1/M2 a `nv-04` (M1 ya venía en migración inicial; M3/M4 sin definir en spec)
- [x] `jti` + blacklist listos para `nv-06`
- [x] Tests + 1 fila en `docs/endpoints.md`
- [x] Puerta `nv-03`: delta informativo + Stryker advisory; global ≥45%
- [x] Tag `v1.0.0-auth5`
- [x] Aprobación firmada: Revisor Usuario Fecha 2026-10-12
