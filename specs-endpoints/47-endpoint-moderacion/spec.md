# 47-endpoint-moderacion — Moderación de contenido y prevención de PII / Prompt Injection

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/moderacion` · Estado: Implementado · Fecha: 2026-11-11.

## 1. Objetivo

Validar comentarios, reportes e URLs de imagen antes de persistirlos, bloqueando groserías, intentos de prompt injection, exposición de PII y esquemas de URL no seguros (Lineamiento 16).

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/moderacion` |
| Autenticación | Anónimo / Rate limiting Global |
| Consumidor | App Móvil / Portal Web / Servicios internos |
| Estado | Implementado |

**Entrada (`ModeracionRequest`):** `texto`, `tipoContenido`, `imagenUrl`.
**Salida (`ModeracionResponse`):** `aprobado`, `motivo`, `puntuacionSeguridad`, `infraccionesDetectadas`.

## 3. Códigos HTTP

- `200 OK`: Evaluación completada.
- `400 Bad Request`: Solicitud sin `texto` ni `imagenUrl`.
- `429 Too Many Requests`: Límite de tasa global superado.

## 4. Seguridad

- Detección regex de prompt injection, términos prohibidos, correos electrónicos, teléfonos y validación `http`/`https` en imágenes.

## 5. Criterios de aceptación

- [x] Controlador implementado en `API_SDD/Controllers/V1/ModeracionController.cs`.
- [x] Pruebas automatizadas en `UnitTest/Controllers/V1EndpointsTests.cs`.

## Aprobación

- [x] Revisión de contrato y seguridad completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con `constitution.md`
