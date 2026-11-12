# 50-endpoint-viajes — Endpoints de viajes guardados (EP-VG-01..05)

> Repositorio: BusTrack API · Endpoints: `GET/POST/PUT/DELETE /api/v1/viajes` · Estado: Implementado · Fecha: 2026-11-12.

## 1. Objetivo

Permitir a los usuarios autenticados guardar, consultar, marcar como favoritos, editar y eliminar sus viajes planeados sincronizables offline.

## 2. Contrato

- **Métodos:** `GET /api/v1/viajes`, `GET /api/v1/viajes/{id}`, `POST /api/v1/viajes`, `PUT /api/v1/viajes/{id}`, `DELETE /api/v1/viajes/{id}`.
- **Auth:** JWT (`Usuario`). **Rate:** `Global` / `ConcurrentWrites`.
- **Moderación previa:** Título y notas validados por `IContentModerationService` antes de almacenar (`422` si incumple).

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-12
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
