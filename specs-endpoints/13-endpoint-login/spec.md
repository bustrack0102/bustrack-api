# 13-endpoint-login — Endpoint de login

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/login` · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Autenticar credenciales y emitir un JWT HS256 con expiración.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/login` |
| Autenticación | JWT HS256 en POST /api/v1/auth/login y POST /api/v1/auth/logout (blacklist en IMemoryCache). |
| Consumidor | Flutter y Angular |
| Estado | Implementado |

**Parámetros / entrada:**

- Body JSON: usuario/correo y contraseña.

**Respuesta:**

- 200: token JWT y expiración.
- 401: credenciales inválidas.
- 423/429: bloqueo o rate limit.

## 3. Códigos HTTP

- 200/201: éxito.
- 400/422: validación.
- 401/403: autenticación/autorización.
- 404: recurso inexistente.
- 409: conflicto.
- 429: rate limit.
- 500: error interno sin detalles.

## 4. Validaciones

1. Usuario y contraseña obligatorios.
2. Longitud y formato válidos.
3. No revelar si el usuario existe.

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
