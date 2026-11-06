# Tareas — 41-endpoint-auth-registro — Endpoint de registro de usuario (EP-AU-01)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTOs y validador de registro

- Diseñar `RegistroRequestDto` y `RegistroResponseDto`.
- Implementar `RegistroRequestValidator` con reglas de correo, coincidencia de password y aceptación de privacidad.

**Archivos:** API_SDD/Dtos/Auth/RegistroRequestDto.cs, API_SDD/Validators/Auth/RegistroRequestValidator.cs

**Terminado cuando:** las clases compilen y pasen validación de modelo.

### T02. Extender IAuthService y AuthController

- Implementar `RegistroAsync` en `AuthService`.
- Exponer endpoint `POST /api/v1/auth/registro`.

**Archivos:** API_SDD/Services/AuthService.cs, API_SDD/Controllers/V1/AuthController.cs

**Terminado cuando:** el endpoint devuelva 201 Created con usuario y par de tokens.

### T03. Pruebas de contrato y unicidad

- Probar registro exitoso, rechazo por contraseña no coincidente y rechazo por correo existente (409).

**Archivos:** UnitTest/Auth/RegistroTests.cs

**Terminado cuando:** la suite de pruebas unitarias pase en verde.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.
