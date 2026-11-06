# 24-endpoint-ping — Endpoint de ping

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/ping` · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Verificar la disponibilidad operativa básica, latencia y respuesta HTTP de la API sin interactuar con la base de datos.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/ping` |
| Autenticación | Ninguna (anónimo) |
| Consumidor | Flutter, Angular, balanceadores de carga y monitores externos |
| Estado | Implementado |

**Parámetros / entrada:**

- Sin parámetros de consulta ni cuerpo.

**Respuesta:**

- 200: Objeto JSON con mensaje de confirmación, timestamp UTC y correlation ID.

```json
{
  "status": "ok",
  "timestamp": "2026-11-05T23:30:00Z"
}
```

## 3. Códigos HTTP

- 200: Operación exitosa.
- 429: Rate limit excedido (política global).
- 500: Error interno no controlado.

## 4. Validaciones

1. Solo responde al método `GET`.
2. Incluye header `X-Correlation-Id` en la respuesta.

## 5. Seguridad

- ASVS L2, rate limiting global por IP, CSP con nonce, Serilog scrubbing y cero secretos en Git.
- No procesa credenciales ni PII.

## 6. Criterios de aceptación

- [x] El endpoint responde `200 OK` con estado "ok" y timestamp.
- [x] Header de correlación presente en la respuesta.
- [x] Trazabilidad con `API_SDD/Controllers/V1/PingController.cs`.
- [ ] `Memoria.md` refleja el estado resultante.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/02-arquitectura-base-y-pipeline-http/`

