# 20-endpoint-paradas — Endpoint de paradas

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/rutas/{id}/paradas` · Estado: Propuesto · Fecha: 2026-10-30.

## 1. Objetivo

Listar paradas ordenadas de una ruta.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/rutas/{id}/paradas` |
| Autenticación | JWT HS256 en POST /api/v1/auth/login y POST /api/v1/auth/logout (blacklist en IMemoryCache). |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Path: id de ruta.

**Respuesta:**

- 200: paradas ordenadas.
- 404: ruta inexistente.

## 3. Códigos HTTP

- 200/201: éxito.
- 400/422: validación.
- 401/403: autenticación/autorización.
- 404: recurso inexistente.
- 409: conflicto.
- 429: rate limit.
- 500: error interno sin detalles.

## 4. Validaciones

1. Ruta existente.
2. Orden y coordenadas válidas.

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

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
