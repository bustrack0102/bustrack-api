# Plan técnico — 56-endpoint-auth-verificar-correo

> Spec: `spec.md` · Endpoint: `GET /api/v1/auth/verificar-correo` · Estado: Implementado.

## 1. Estrategia

Incorporar la acción `VerificarCorreo` en `AuthController`, delegando la validación del hash criptográfico a `IAuthService` con soporte para `AppDbContext` (SQL Server) y `IBusTrackNoSqlStore` (MongoDB Atlas / In-Memory).

## 2. Componentes

- `AuthController`: Acción HTTP GET con `[AllowAnonymous]` y `[EnableRateLimiting("Login")]`.
- `IAuthService` / `AuthService` / `MongoAuthService`: Lógica de verificación y activación.
- `CryptoTokenHelper`: Hash SHA-256 para búsqueda en índice único.
- `VerificacionCorreoResponseDto`: Modelo de respuesta limpio.

## 3. Pruebas

- Pruebas unitarias de tokens (`UnitTest/Auth/EmailVerificationTokenTests.cs`).
- Pruebas de integración (`IntegrationTest/EmailVerificationIntegrationTests.cs`).
