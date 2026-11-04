# 16-endpoint-usuarios-coleccion — Endpoint de colección de usuarios

> Repositorio: BusTrack API · Endpoint: `GET/POST /api/v1/usuarios` · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Listar usuarios paginados y crear usuarios con hash Argon2id.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET/POST` |
| Ruta | `/api/v1/usuarios` |
| Autenticación | JWT con AdminPolicy. |
| Consumidor | Panel administrativo |
| Estado | Implementado |

**Parámetros / entrada:**

- GET: page, pageSize, filtros.
- POST: DTO de usuario.

**Respuesta:**

- 200: página de usuarios.
- 201: usuario creado.
- 409: duplicado.

## 3. Códigos HTTP

- 200/201: éxito.
- 400/422: validación.
- 401/403: autenticación/autorización.
- 404: recurso inexistente.
- 409: conflicto.
- 429: rate limit.
- 500: error interno sin detalles.

## 4. Validaciones

1. Unicidad de usuario/correo.
2. Contraseña con política.
3. Paginación acotada.

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
