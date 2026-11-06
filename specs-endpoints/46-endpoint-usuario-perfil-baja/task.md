# Tareas — 46-endpoint-usuario-perfil-baja — Endpoint de baja de cuenta de usuario autenticado (EP-AU-09)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTO y validador de baja de cuenta

- Crear `BajaCuentaRequestDto` con `passwordConfirmacion`.
- Validar presencia y formato con FluentValidation.

**Archivos:** API_SDD/Dtos/Usuario/BajaCuentaRequestDto.cs, API_SDD/Validators/Usuario/BajaCuentaValidator.cs

**Terminado cuando:** las clases compilen y pasen validación de campo requerido.

### T02. Pruebas de verificación de contraseña y anonimización

- Probar rechazo con 400 si la contraseña de confirmación no coincide con el hash del usuario.
- Probar éxito (200), marcando `activo = false`, anonimizando campos PII e invalidando los refresh tokens asociados.

**Archivos:** UnitTest/Usuario/BajaCuentaTests.cs

**Terminado cuando:** la suite de pruebas demuestre la anonimización y la imposibilidad de volver a iniciar sesión con esa cuenta.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.
