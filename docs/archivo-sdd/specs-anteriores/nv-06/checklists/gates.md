# Checklist — nv-06 Logout (revocación JWT)

> Trazabilidad: `spec §1-§7 ↔ plan ↔ task`. Estado: ✅ Aprobada y firmada por Usuario 2026-10-12 (merge PR #5).
> Archivo de higiene SDD (`check_sdd.ps1` B1 exige `checklists/` por spec). Contenido derivado de `spec.md §2`, sin ampliar alcance.

- [x] `POST /api/v1/auth/logout [Authorize]` → `204`
- [x] `blacklist:{jti}` en `IMemoryCache` con TTL = vida remanente; reúso → `401`
- [x] `ITokenRevocationService`; sin estado en SQL, sin superficie nueva
- [x] Reutiliza `OnTokenValidated` anti-blacklist ya cableado
- [x] Tests + 1 fila en `docs/endpoints.md`
- [x] Puerta `nv-03`: delta informativo + Stryker advisory; global ≥45%
- [x] Tag `v1.0.0-auth6`
- [x] Aprobación firmada: Revisor Usuario Fecha 2026-10-12
