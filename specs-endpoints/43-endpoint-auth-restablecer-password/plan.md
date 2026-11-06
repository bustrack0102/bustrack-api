# Plan técnico — 43-endpoint-auth-restablecer-password — Endpoint de restablecimiento de contraseña (EP-AU-06)

> Spec: `spec.md` · Endpoint: `POST /api/v1/auth/restablecer-password` · Estado: Propuesto.

## 1. Estrategia

Diseñar la acción `RestablecerPasswordAsync` en `AuthController`, ejecutando una transacción que actualice el hash de credenciales en `usuarios`, queme el token en `tokens_recuperacion` y revoque los registros en `refresh_tokens`.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, Argon2id.
- Componentes: `AuthController`, `IRestablecerPasswordService`, `IPasswordHasher`, `RestablecerPasswordRequestDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/AuthController.cs
API_SDD/Dtos/Auth/RestablecerPasswordRequestDto.cs
API_SDD/Validators/Auth/RestablecerPasswordValidator.cs
API_SDD/Services/Auth/IRestablecerPasswordService.cs
```

## 4. Seguridad

- ASVS L2, hash de token SHA-256 para verificación en BD.
- Revocación de sesiones en cascada.

## 5. Errores

- Retorno de RFC 7807 uniforme para 400 Bad Request y 422 Unprocessable.

## 6. Pruebas

- Pruebas unitarias de coincidencia de password y expiración de token.
- Pruebas de integración verificando revocación en cascada de refresh tokens.

## 7. Riesgos

- Reuso de tokens neutralizado mediante flag `usado = true` en la misma transacción de actualización de clave.
