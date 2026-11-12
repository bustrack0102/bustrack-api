# 52-endpoint-dispositivos-fcm — Endpoints de dispositivos Push FCM (Req. 16)

> Repositorio: BusTrack API · Endpoints: `GET /api/v1/dispositivos/mios`, `POST /api/v1/dispositivos`, `DELETE /api/v1/dispositivos/{id}` · Estado: Implementado · Fecha: 2026-11-12.

## 1. Objetivo

Registrar y administrar los dispositivos móviles habilitados para notificaciones Push vía Firebase Cloud Messaging HTTP v1.

## 2. Seguridad y Criterios

- Sin exponer claves FCM ni registrar credenciales en logs (`B2` critic).
- Desacoplado con resiliencia HTTP Polly cuando Firebase está configurado.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-12
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
