# 23-endpoint-calificaciones — Endpoint de calificaciones

> Repositorio: BusTrack API · Endpoint: `GET/POST /api/v1/calificaciones` · Estado: Propuesto · Fecha: 2026-10-30.

## 1. Objetivo

Registrar calificaciones y consultar agregados por empresa/servicio.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET/POST` |
| Ruta | `/api/v1/calificaciones` |
| Autenticación | JWT de usuario. |
| Consumidor | Flutter |
| Estado | Propuesto |

**Parámetros / entrada:**

- POST: empresa/servicio, puntuación, comentario.
- GET: filtros y paginación.

**Respuesta:**

- 201: creada.
- 200: agregados.
- 409: duplicada.
- 422: validación.

## 3. Códigos HTTP

- 200/201: éxito.
- 400/422: validación.
- 401/403: autenticación/autorización.
- 404: recurso inexistente.
- 409: conflicto.
- 429: rate limit.
- 500: error interno sin detalles.

## 4. Validaciones

1. Rango de puntuación.
2. Una por usuario/servicio/período.
3. Comentario acotado.

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
