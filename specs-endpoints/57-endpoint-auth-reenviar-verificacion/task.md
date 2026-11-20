# Tareas — 57-endpoint-auth-reenviar-verificacion

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.

## Actividades

### T01. Modelar DTO y contratos
- `ReenviarVerificacionRequestDto` agregado a `AuthDtos.cs`.
- Declarar método en `IAuthService`.

### T02. Implementar servicio en SQL Server y NoSQL
- Búsqueda por correo normalizado.
- Verificación de que la cuenta no esté ya verificada.
- Invalidación de tokens pendientes previos.
- Generación y almacenamiento de nuevo token seguro.
- Envío de correo electrónico.

### T03. Exponer endpoint en AuthController
- Ruta `POST /api/v1/auth/reenviar-verificacion`.
- Validación y respuesta uniforme.
