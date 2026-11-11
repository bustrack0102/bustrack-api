# 26-endpoint-documentacion-openapi — Endpoints de documentación OpenAPI y Scalar

> Repositorio: BusTrack API · Endpoint: `GET /openapi/v1.json` y `GET /scalar/v1` · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Exponer la especificación de OpenAPI 3.0 v1 y la interfaz visual interactiva de Scalar para documentación y pruebas de desarrollo, restringido únicamente a ambientes de desarrollo.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/openapi/v1.json` y `/scalar/v1` |
| Autenticación | Anónimo en desarrollo; bloqueado en producción |
| Consumidor | Desarrolladores web/móvil, herramientas de generación de clientes |
| Estado | Implementado |

**Rutas:**

- `GET /openapi/v1.json`: Documento JSON con el esquema OpenAPI v1 de la API.
- `GET /scalar/v1`: Interfaz web interactiva Scalar para probar peticiones y explorar contratos.

**Respuesta:**

- 200: En Development, contenido JSON OpenAPI o HTML Scalar.
- 404: Fuera del entorno de desarrollo.

## 3. Códigos HTTP

- 200: Éxito (solo Development).
- 404: Recurso no encontrado (en Staging y Production).
- 429: Rate limit excedido.

## 4. Validaciones

1. Solo disponible si `app.Environment.IsDevelopment()` es verdadero.
2. Contratos y DTOs generados con esquemas de validación precisos.

## 5. Seguridad

- Ningún detalle interno, endpoint de diagnóstico ni documentación expuesto en producción.
- No almacena ni procesa tokens ni secretos.

## 6. Criterios de aceptación

- [x] `/openapi/v1.json` genera un esquema válido OpenAPI 3.0.
- [x] `/scalar/v1` renderiza la UI interactiva.
- [x] En producción ambas rutas retornan 404.

## 7. Dependencias

- `constitution.md`
- `docs/endpoints.md`
- `specs/02-arquitectura-base-y-pipeline-http/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
