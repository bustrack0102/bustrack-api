# 25-endpoint-probes — Endpoints de diagnóstico y sondeo de errores

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/probe/{timeout|error|forbidden|conflict|invalid}` · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Exponer sondas de diagnóstico en entornos no productivos para simular y validar las respuestas de error estandarizadas bajo RFC 7807 (Problem Details) en los clientes (Flutter y Angular).

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/probe/{timeout\|error\|forbidden\|conflict\|invalid}` |
| Autenticación | Ninguna en entornos no productivos; bloqueado en producción |
| Consumidor | Suites de pruebas automatizadas y desarrolladores de clientes |
| Estado | Implementado |

**Rutas específicas:**

- `GET /api/v1/probe/timeout`: Simula expiración de operación o petición tardía (408).
- `GET /api/v1/probe/error`: Simula excepción interna no controlada (500).
- `GET /api/v1/probe/forbidden`: Simula denegación de autorización (403).
- `GET /api/v1/probe/conflict`: Simula conflicto de concurrencia o unicidad (409).
- `GET /api/v1/probe/invalid`: Simula error de validación de modelo o negocio (422).

**Respuesta:**

- En desarrollo/pruebas: RFC 7807 `ProblemDetails` con el código HTTP correspondiente.
- En producción: 404 Not Found para salvaguardar la superficie de ataque.

## 3. Códigos HTTP

- 408: Request Timeout simulado.
- 500: Internal Server Error simulado.
- 403: Forbidden simulado.
- 409: Conflict simulado.
- 422: Unprocessable Entity simulado.
- 404: Not Found si el entorno es Production.
- 429: Rate limit excedido.

## 4. Validaciones

1. Desactivado en ambiente de producción (`IHostEnvironment.IsProduction()`).
2. Retorno de estructura `ProblemDetails` estándar sin fugas de stack trace sensible.

## 5. Seguridad

- Restricción estricta por entorno.
- Sanitización de mensajes con Serilog scrubbing.
- Cabeceras de seguridad activas.

## 6. Criterios de aceptación

- [x] Cada sonda dispara el código de estado RFC 7807 esperado en desarrollo.
- [x] En ambiente de producción las rutas devuelven 404 Not Found.
- [x] Trazabilidad con `API_SDD/Controllers/V1/ProbeController.cs`.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/02-arquitectura-base-y-pipeline-http/`

