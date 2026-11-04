# 17-endpoint-usuario-individual — Endpoint de usuario individual

> Repositorio: BusTrack API · Endpoint: `GET/PUT/DELETE /api/v1/usuarios/{id}` · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Consultar, actualizar y eliminar un usuario por identificador.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET/PUT/DELETE` |
| Ruta | `/api/v1/usuarios/{id}` |
| Autenticación | JWT con AdminPolicy. |
| Consumidor | Panel administrativo |
| Estado | Implementado |

**Parámetros / entrada:**

- Path: id.
- PUT: DTO y RowVersion.

**Respuesta:**

- 200: usuario.
- 204: eliminado.
- 404: inexistente.
- 409: concurrencia.

## 3. Códigos HTTP

- 200/201: éxito.
- 400/422: validación.
- 401/403: autenticación/autorización.
- 404: recurso inexistente.
- 409: conflicto.
- 429: rate limit.
- 500: error interno sin detalles.

## 4. Validaciones

1. Id válido.
2. RowVersion en actualización.
3. No permitir auto-eliminación del último admin.

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
