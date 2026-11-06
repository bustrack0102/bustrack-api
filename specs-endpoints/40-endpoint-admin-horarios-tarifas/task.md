# Tareas — 40-endpoint-admin-horarios-tarifas — Endpoint de administración de horarios y tarifas de ruta

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTOs y validadores de horarios y tarifas

- Diseñar `CrearHorarioDto` y `CrearTarifaDto`.
- Implementar validadores FluentValidation que detecten montos negativos o rangos de hora invertidos.

**Archivos:** API_SDD/Dtos/Transporte/CrearTarifaDto.cs, API_SDD/Validators/Transporte/CrearTarifaValidator.cs

**Terminado cuando:** los DTOs y validadores estén implementados y compilando.

### T02. Pruebas de validación temporal y autorización

- Probar rechazo ante solapamiento de horarios para el mismo día.
- Probar que un usuario no administrador recibe 403 Forbidden.

**Archivos:** UnitTest/Transporte/HorariosTarifasAdminTests.cs

**Terminado cuando:** las pruebas unitarias cubran los casos de negocio y seguridad.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

