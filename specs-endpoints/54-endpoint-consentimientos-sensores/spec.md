# 54-endpoint-consentimientos-sensores — Consentimientos, Sensores ML y Candidatos (EP-ML-01..07, EP-AD-08)

> Repositorio: BusTrack API · Endpoints: `/api/v1/consentimientos/*`, `/api/v1/sensores/lotes`, `/api/v1/candidatos-parada`, `/api/v1/admin/candidatos-parada/{id}/validar` · Estado: Implementado · Fecha: 2026-11-12.

## 1. Objetivo

Controlar el consentimiento explícito revocable de sensores inerciales, procesar lotes anónimos con `bustrack-ml`, registrar paradas candidatas y permitir su promoción oficial con notificación Push FCM.

## 2. Seguridad

- Bloquea ingesta sensorial (`403`) si el usuario no otorgó consentimiento activo.
- AdminPolicy obligatoria para revisión de lotes, bandeja de candidatos y aprobación (`EP-AD-08`).

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-12
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
