# 53-endpoint-admin-fuentes-importaciones — Endpoints admin fuentes e importaciones (EP-AD-02..07)

> Repositorio: BusTrack API · Endpoints: `/api/v1/admin/fuentes-datos` y `/api/v1/admin/importaciones` · Estado: Implementado · Fecha: 2026-11-12.

## 1. Objetivo

Gestionar el catálogo oficial de fuentes de datos (`EP-AD-02..05`) y ejecutar/auditar importaciones (`EP-AD-06..07`) con invalidación automática de caché Redis.

## 2. Seguridad y Criterios

- Estricto `AdminPolicy` (`role: Admin`), validado con rate limit `Admin` / `ConcurrentWrites`.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-12
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
