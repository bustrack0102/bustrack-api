# 14-endpoint-logout — Endpoint de logout

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/logout` · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Revocar el token actual mediante blacklist en memoria.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/logout` |
| Autenticación | JWT HS256 en POST /api/v1/auth/login y POST /api/v1/auth/logout (blacklist en IMemoryCache). |
| Consumidor | Flutter y Angular |
| Estado | Implementado |

**Parámetros / entrada:**

- Header Authorization: Bearer JWT.
- Sin body.

**Respuesta:**

- 204: revocado.
- 401: token ausente o inválido.

## 3. Códigos HTTP

- 200/201: éxito.
- 400/422: validación.
- 401/403: autenticación/autorización.
- 404: recurso inexistente.
- 409: conflicto.
- 429: rate limit.
- 500: error interno sin detalles.

## 4. Validaciones

1. Token válido y no expirado.
2. Idempotente.

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
