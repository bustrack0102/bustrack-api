# Tareas — 42-endpoint-auth-recuperar-password — Endpoint de solicitud de recuperación de contraseña (EP-AU-05)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTO y servicio de recuperación

- Crear `RecuperarPasswordRequestDto`.
- Definir interfaz `IRecuperacionPasswordService` con método `SolicitarRecuperacionAsync(string correo)`.

**Archivos:** API_SDD/Dtos/Auth/RecuperarPasswordRequestDto.cs, API_SDD/Services/Auth/IRecuperacionPasswordService.cs

**Terminado cuando:** las clases y métodos estén definidos y compilando.

### T02. Pruebas de anti-enumeración y generación de tokens

- Probar que el retorno sea 200 OK con el mismo mensaje en caso de éxito o de correo inexistente.
- Probar que el token generado tenga vigencia de exactamente 30 minutos y se almacene únicamente su hash.

**Archivos:** UnitTest/Auth/RecuperacionPasswordTests.cs

**Terminado cuando:** las pruebas unitarias pasen satisfactoriamente.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.
