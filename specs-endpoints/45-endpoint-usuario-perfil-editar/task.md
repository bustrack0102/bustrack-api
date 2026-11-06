# Tareas — 45-endpoint-usuario-perfil-editar — Endpoint de edición de perfil de usuario autenticado (EP-AU-08)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTOs y validador de edición de perfil

- Diseñar `EditarPerfilRequestDto` y `PerfilActualizadoResponseDto`.
- Implementar `EditarPerfilValidator` para reglas condicionales de contraseña.

**Archivos:** API_SDD/Dtos/Usuario/EditarPerfilRequestDto.cs, API_SDD/Validators/Usuario/EditarPerfilValidator.cs

**Terminado cuando:** el validador exija `passwordActual` si `nuevaPassword` está presente.

### T02. Pruebas de unicidad de correo y contraseña

- Probar actualización correcta de nombre y correo.
- Probar rechazo con 409 si el correo nuevo ya existe en otra cuenta.
- Probar rechazo con 400 si `passwordActual` no coincide con el hash del usuario.

**Archivos:** UnitTest/Usuario/EditarPerfilTests.cs

**Terminado cuando:** las pruebas unitarias pasen en verde.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.
