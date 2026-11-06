# Plan técnico — 45-endpoint-usuario-perfil-editar — Endpoint de edición de perfil de usuario autenticado (EP-AU-08)

> Spec: `spec.md` · Endpoint: `PUT /api/v1/usuarios/me` · Estado: Propuesto.

## 1. Estrategia

Diseñar la acción `UpdateProfileAsync` en `UsuariosController` protegida con `[Authorize]`, inyectando validador de actualización de perfil y verificando contraseña actual si se modifica la clave.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, Argon2id.
- Componentes: `UsuariosController`, `IUsuarioService`, `EditarPerfilRequestDto`, `PerfilActualizadoResponseDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/UsuariosController.cs
API_SDD/Dtos/Usuario/EditarPerfilRequestDto.cs
API_SDD/Dtos/Usuario/PerfilActualizadoResponseDto.cs
API_SDD/Validators/Usuario/EditarPerfilValidator.cs
API_SDD/Services/IUsuarioService.cs
```

## 4. Seguridad

- ASVS L2, validación de hash de contraseña actual previa a cualquier reemplazo de credencial.
- Comprobación de colisión de correo en BD.

## 5. Errores

- Retorno RFC 7807 uniforme para 409 Conflict y 400 Bad Request.

## 6. Pruebas

- Pruebas unitarias de validación con FluentValidation.
- Pruebas de integración para cambio exitoso de nombre, rechazo por correo duplicado y validación de contraseña actual.

## 7. Riesgos

- Modificación concurrente mitigada por control de concurrencia optimista o actualización atómica.
