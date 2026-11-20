# Tareas — 56-endpoint-auth-verificar-correo

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.

## Actividades

### T01. Modelar DTO y métodos en contratos
- `VerificacionCorreoResponseDto` agregado a `AuthDtos.cs`.
- Declarar `VerificarCorreoAsync` en `IAuthService`.

### T02. Implementar servicio en SQL Server y NoSQL
- Búsqueda por `TokenHash` SHA-256.
- Validación de expiración y estado de uso.
- Activación de cuenta (`Activo = true`, `CorreoVerificado = true`).

### T03. Exponer endpoint en AuthController y pruebas
- Ruta `GET /api/v1/auth/verificar-correo?token=...`.
- Validación de rate limiting y respuestas consistentes.
