# Plan técnico — 41-endpoint-auth-registro — Endpoint de registro de usuario (EP-AU-01)

> Spec: `spec.md` · Endpoint: `POST /api/v1/auth/registro` · Estado: Propuesto.

## 1. Estrategia

Extender `AuthController` con la acción pública `RegistroAsync`, reutilizando `IPasswordHasher` (Argon2id), `IJwtTokenService` para el JWT y el servicio de persistencia de tokens de renovación.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL (o EF Core durante la transición actual).
- Componentes: `AuthController`, `IAuthService`, `RegistroRequestDto`, `RegistroResponseDto`, `RegistroRequestValidator`.
- Transacción: Inserción en `usuarios` vinculando rol `Usuario` y registro en `refresh_tokens`.

## 3. Archivos

```text
API_SDD/Controllers/V1/AuthController.cs
API_SDD/Dtos/Auth/RegistroRequestDto.cs
API_SDD/Dtos/Auth/RegistroResponseDto.cs
API_SDD/Validators/Auth/RegistroRequestValidator.cs
API_SDD/Services/IAuthService.cs
API_SDD/Services/AuthService.cs
```

## 4. Seguridad

- ASVS L2, Argon2id con parámetros recomendados de memoria e iteraciones.
- Rate limiting con política "Login" o "Registro" restrictiva.

## 5. Errores

- Retorno RFC 7807 uniforme para 409 Conflict y 400 Bad Request.

## 6. Pruebas

- Pruebas unitarias de validación con FluentValidation.
- Pruebas de integración de registro exitoso (201) y detección de duplicado (409).

## 7. Riesgos

- Ataques de registro automatizado mitigados con rate limiting y verificación estricta de políticas.
