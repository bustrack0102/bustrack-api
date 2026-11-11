# 33-endpoint-auth-refresh — Endpoint de refresco y renovación de tokens JWT

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/refresh` · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Renovar un token de acceso JWT HS256 expirado utilizando un token de refresco criptográfico (Refresh Token) válido y aplicando rotación automática de tokens para mantener sesiones móviles/web seguras.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/refresh` |
| Autenticación | Refresh Token en cuerpo o encabezado |
| Consumidor | Flutter y Angular |
| Estado | Implementado |

**Parámetros / entrada:**

- Body JSON con el token de acceso expirado y el token de refresco:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "d8e8fca2dc6b4bf09c48b2a3fa42e1bb45..."
}
```

**Respuesta:**

- 200: Nuevo par de tokens (Access Token renovado y nuevo Refresh Token rotado).
- 400: Solicitud mal formada o parámetros faltantes.
- 401: Refresh Token inválido, expirado, revocado o detección de reutilización indebida (token reuse detection).

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "refreshToken": "a1b2c3d4e5f67890abcdef1234567890..."
}
```

## 3. Códigos HTTP

- 200: Token renovado con éxito.
- 400: Estructura del body inválida.
- 401: Refresh Token revocado o no coincide con el hash almacenado.
- 429: Rate limit de autenticación excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. `accessToken` y `refreshToken` obligatorios y no vacíos.
2. Detección de reutilización de refresh tokens: si un token ya utilizado vuelve a presentarse, se invalidan inmediatamente todas las sesiones del usuario.
3. El refresh token se almacena con hash criptográfico unidireccional (Argon2id o SHA-256 seguro con sal).

## 5. Seguridad

- ASVS L2, rotación obligatoria de token de refresco en cada llamada exitosa.
- Revocación en cascada ante intento de reutilización fraudulenta.
- Rate limiting específico para endpoints de autenticación.

## 6. Criterios de aceptación

- [ ] Genera un nuevo JWT y nuevo refresh token ante credenciales válidas.
- [ ] Invalida el refresh token anterior una vez emitido el nuevo.
- [ ] Devuelve 401 si el token expiró o fue manipulado.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/05-autenticacion-jwt/`
- `specs-endpoints/13-endpoint-login/`

