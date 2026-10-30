# Checklist — nv-04 CRUD SDDUsuario

> Trazabilidad: `spec §1-§7 ↔ plan ↔ task`. Estado: ✅ Aprobada y firmada (Usuario, 2026-10-12).
> Archivo de higiene SDD (`check_sdd.ps1` B1 exige `checklists/` por spec). Contenido derivado de `spec.md §2`, sin ampliar alcance.

- [x] Entidad `SDDUsuario` + migración (sin `_`) + `DbSet`
- [x] DTOs + validators espejo (registro por reflexión, molde `nv-02`)
- [x] `ISDDUsuarioService` + `SDDUsuariosController` v1.0 + `RowVersion` en Update/Delete
- [x] Argon2id real; DTOs sin hash ni plano (DevSecOps)
- [x] 5 suites verdes + 5 filas en `docs/endpoints.md`
- [x] Puerta `nv-03`: delta informativo + Stryker advisory; global ≥45%
- [x] `Memoria.md` + tag `v1.0.0-crud4`
- [x] Aprobación firmada: Revisor Usuario Fecha 2026-10-12
