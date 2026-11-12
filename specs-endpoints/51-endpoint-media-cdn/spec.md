# 51-endpoint-media-cdn — Endpoint de carga de medios moderados a Blob CDN

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/media/upload` · Estado: Implementado · Fecha: 2026-11-12.

## 1. Objetivo

Validar imágenes (JPEG/PNG/WebP) y videos cortos (MP4/WebM) con `IContentModerationService` (texto/imagen/video) antes de subirlos a Azure Blob Storage con cabecera CDN `Cache-Control: public, max-age=31536000, immutable`.

## 2. Contrato

- **Método:** `POST /api/v1/media/upload` (`multipart/form-data`: `archivo`, `categoria`, `previewFrame`, `notas`).
- **Códigos:** `201 Created` (URL HTTPS + Cache-Control) / `400` / `401` / `422 content_moderation_rejected`.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-12
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
