# 18-endpoint-empresas — Endpoint de empresas y asociaciones

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/empresas` · Estado: Propuesto · Fecha: 2026-10-30.

## 1. Objetivo

Listar empresas, asociaciones y medios de transporte.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/empresas` |
| Autenticación | JWT HS256 en POST /api/v1/auth/login y POST /api/v1/auth/logout (blacklist en IMemoryCache). |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Query: página, tamaño, filtro.

**Respuesta:**

- 200: lista paginada.
- 400: filtro inválido.

## 3. Códigos HTTP

- 200/201: éxito.
- 400/422: validación.
- 401/403: autenticación/autorización.
- 404: recurso inexistente.
- 409: conflicto.
- 429: rate limit.
- 500: error interno sin detalles.

## 4. Validaciones

1. Paginación acotada.
2. Filtros permitidos.

## 5. Seguridad

- ASVS L2, Argon2id, validación JWT estricta, rate limiting, CSP con nonce, Serilog scrubbing y cero secretos en Git.
- No registrar tokens, credenciales ni PII.

## 6. Criterios de aceptación

- [ ] La tarea tiene criterios verificables y evidencia asociada.
- [ ] No se declara implementado lo que no existe en código o pruebas.
- [ ] Los contratos consumidos o expuestos quedan documentados.
- [ ] `Memoria.md` refleja el estado resultante.

## 7. Dependencias

- `docs/contratos-integracion.md`
- `spec.md` de la tarea funcional asociada
