# Plan técnico — 33-endpoint-auth-refresh — Endpoint de refresco y renovación de tokens JWT

> Spec: `spec.md` · Endpoint: `POST /api/v1/auth/refresh` · Estado: Propuesto.

## 1. Estrategia

Extender `AuthController` con la acción `RefreshAsync`, incorporando el servicio de gestión de tokens de refresco con rotación y hash de almacenamiento.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, JWT Bearer HS256.
- Componentes: `AuthController`, `ITokenService`, `RefreshTokenRequestDto`, `RefreshTokenResponseDto`.
- Almacenamiento: Entidad o documento `RefreshToken` asociado al usuario con estado de revocación y fecha de vencimiento.

## 3. Archivos

```text
API_SDD/Controllers/V1/AuthController.cs
API_SDD/Dtos/Auth/RefreshTokenRequestDto.cs
API_SDD/Dtos/Auth/RefreshTokenResponseDto.cs
API_SDD/Security/ITokenService.cs
specs/05-autenticacion-jwt/
```

## 4. Seguridad

- ASVS L2, rotación obligatoria de token de refresco.
- Detección de token reuse revocando la familia de tokens activa.
- Cero almacenamiento de tokens en texto plano en la base de datos.

## 5. Errores

- Retorno 401 ProblemDetails sin revelar detalles de hashes internos.

## 6. Pruebas

- Pruebas unitarias de rotación de tokens y expiración.
- Prueba de detección de token reuse.

## 7. Riesgos

- Carrera en peticiones concurrentes de refresh resuelta mediante bloqueo pesimista o margen de tolerancia de pocos segundos (grace period).

