# Plan técnico — 46-endpoint-usuario-perfil-baja — Endpoint de baja de cuenta de usuario autenticado (EP-AU-09)

> Spec: `spec.md` · Endpoint: `DELETE /api/v1/usuarios/me` · Estado: Propuesto.

## 1. Estrategia

Diseñar la acción `DeleteAccountAsync` en `UsuariosController` protegida con `[Authorize]`, que reciba el body con `passwordConfirmacion`, verifique la identidad con `IPasswordHasher` y ejecute una transacción de anonimización y revocación en cascada.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, Argon2id.
- Componentes: `UsuariosController`, `IUsuarioService`, `BajaCuentaRequestDto`.
- Transacción: Actualización en `usuarios` + borrado en `refresh_tokens`, `tokens_recuperacion`, `viajes_guardados`.

## 3. Archivos

```text
API_SDD/Controllers/V1/UsuariosController.cs
API_SDD/Dtos/Usuario/BajaCuentaRequestDto.cs
API_SDD/Validators/Usuario/BajaCuentaValidator.cs
API_SDD/Services/IUsuarioService.cs
```

## 4. Seguridad

- ASVS L2, exigencia de contraseña actual para mitigar ataques de sesión secuestrada.
- Anonimización de datos en cumplimiento de GDPR/derechos ARCO.

## 5. Errores

- Retorno de 400 Bad Request en contraseña no coincidente.

## 6. Pruebas

- Pruebas unitarias de verificación de contraseña previa.
- Pruebas de integración de baja atómica, revocación de tokens y anonimización de correo y nombre.

## 7. Riesgos

- Corrupción de datos históricos mitigada manteniendo el historial agregado de calificaciones con usuario desvinculado.
