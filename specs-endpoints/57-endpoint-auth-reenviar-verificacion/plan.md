# Plan técnico — 57-endpoint-auth-reenviar-verificacion

> Spec: `spec.md` · Endpoint: `POST /api/v1/auth/reenviar-verificacion` · Estado: Implementado.

## 1. Estrategia

Incorporar la acción `ReenviarVerificacion` en `AuthController`, delegando en `IAuthService` la verificación del estado del usuario, la invalidación de tokens vigentes anteriores y la emisión de uno nuevo con notificación SMTP sin revelar si la cuenta existe.

## 2. Componentes

- `AuthController`: Acción HTTP POST con `[AllowAnonymous]` y `[EnableRateLimiting("Login")]`.
- `ReenviarVerificacionRequestDto`: DTO de entrada.
- `IEmailNotificationService`: Envío de plantilla responsive HTML.
- Persistencia dual (`AppDbContext` / `IBusTrackNoSqlStore`).
