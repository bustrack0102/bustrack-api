# Tareas — 44-endpoint-usuario-perfil-me — Endpoint de consulta de perfil de usuario autenticado (EP-AU-07)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar UsuarioPerfilDto y endpoint

- Crear `UsuarioPerfilDto` con datos de cuenta, rol, estado de sensores y totales de viajes y calificaciones.
- Declarar `GET /api/v1/usuarios/me` en el controlador con atributo `[Authorize]`.

**Archivos:** API_SDD/Dtos/Usuario/UsuarioPerfilDto.cs, API_SDD/Controllers/V1/UsuariosController.cs

**Terminado cuando:** el DTO y la signatura del endpoint estén definidos.

### T02. Pruebas de autenticación y lectura

- Probar retorno 200 con token de usuario válido.
- Probar rechazo con 401 si la petición no incluye cabecera Authorization válida.

**Archivos:** UnitTest/Usuario/UsuarioPerfilTests.cs

**Terminado cuando:** las pruebas unitarias y de integración pasen satisfactoriamente.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.
