# 43-endpoint-auth-restablecer-password — Endpoint de restablecimiento de contraseña (EP-AU-06)

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/restablecer-password` · Código: EP-AU-06 · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Completar el cambio de contraseña olvidada desde la Pantalla de Restablecimiento de Contraseña (BE-03, MO-11), validando el token de recuperación, verificando la fortaleza de la nueva contraseña, actualizando el hash con Argon2id y revocando por seguridad todos los Refresh Tokens activos del usuario.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/restablecer-password` |
| Código Tarea | `EP-AU-06` |
| Autenticación | Público (con Token de recuperación) |
| Consumidor | Flutter (Móvil) y Angular (Web) |
| Estado | Implementado |

**Parámetros / entrada:**

- Body JSON:
```json
{
  "token": "a9b8c7d6e5f41234...",
  "nuevaPassword": "NuevoPassword123!",
  "confirmacionPassword": "NuevoPassword123!"
}
```

**Respuesta (200 OK):**
```json
{
  "mensaje": "Contraseña actualizada correctamente. Inicia sesión con tu nueva clave."
}
```

## 3. Códigos HTTP

- 200: Contraseña actualizada y sesiones anteriores revocadas.
- 400: Token inválido, expirado, ya utilizado o contraseñas no coincidentes.
- 422: Contraseña no cumple las políticas de complejidad.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. `token`, `nuevaPassword` y `confirmacionPassword` obligatorios.
2. `nuevaPassword` idéntico a `confirmacionPassword` y con mínimo 8 caracteres conteniendo letras y números.
3. El hash del token debe existir en `tokens_recuperacion`, tener `usado == false` y `fecha_expiracion > DateTime.UtcNow`.
4. Operación atómica/transaccional:
   - Actualiza hash de contraseña del usuario en `usuarios` usando Argon2id.
   - Marca el token como usado (`usado = true`, `fecha_uso = DateTime.UtcNow`).
   - Revoca todos los Refresh Tokens vigentes del usuario en `refresh_tokens`.

## 5. Seguridad

- ASVS L2, Argon2id, revocación inmediata de sesiones activas concurrentes para evitar accesos indebidos tras un restablecimiento.
- El token queda invalidado de inmediato tras su primer uso.

## 6. Criterios de aceptación

- [ ] Actualiza la contraseña y marca el token como usado.
- [ ] Revoca todos los Refresh Tokens activos del usuario.
- [ ] Rechaza con 400 Bad Request si el token expiró o ya fue consumido.

## 7. Dependencias

- `constitution.md`
- `specs/05-autenticacion-jwt/`
- `specs-endpoints/14-endpoint-logout/`
- `specs-endpoints/42-endpoint-auth-recuperar-password/`
