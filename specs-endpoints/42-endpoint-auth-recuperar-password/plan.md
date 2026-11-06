# Plan técnico — 42-endpoint-auth-recuperar-password — Endpoint de solicitud de recuperación de contraseña (EP-AU-05)

> Spec: `spec.md` · Endpoint: `POST /api/v1/auth/recuperar-password` · Estado: Propuesto.

## 1. Estrategia

Diseñar endpoint en `AuthController` acoplado con `IRecuperacionPasswordService` y `IEmailService` para coordinar la generación de tokens de un solo uso y notificación por correo con mitigación de enumeración.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL / SQL.
- Componentes: `AuthController`, `IRecuperacionPasswordService`, `IEmailSender`, `RecuperarPasswordRequestDto`.
- Almacenamiento: Registro en `tokens_recuperacion` con hash SHA-256 del token, fecha de creación, expiración (+30 min) y usado = false.

## 3. Archivos

```text
API_SDD/Controllers/V1/AuthController.cs
API_SDD/Dtos/Auth/RecuperarPasswordRequestDto.cs
API_SDD/Services/Auth/IRecuperacionPasswordService.cs
API_SDD/Services/Email/IEmailSender.cs
```

## 4. Seguridad

- ASVS L2, respuesta idéntica y tiempo de respuesta constante para evitar ataques de temporización.
- Rate limiting restrictivo.

## 5. Errores

- Retorno uniforme 200 ante peticiones válidas en sintaxis.

## 6. Pruebas

- Pruebas unitarias de hash y cálculo de expiración (30 min).
- Pruebas de integración validando mensaje uniforme para usuarios existentes y no existentes.

## 7. Riesgos

- Abuso de envío de correos mitigado por rate limit estricto por IP y correo.
