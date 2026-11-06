# Tareas — 30-endpoint-medios-transporte — Endpoint de medios y modalidades de transporte

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Definir DTO y Controlador

- Especificar estructura `MedioTransporteDto`.
- Implementar `GET /api/v1/medios-transporte`.

**Archivos:** API_SDD/Dtos/Transporte/MedioTransporteDto.cs, API_SDD/Controllers/V1/MediosTransporteController.cs

**Terminado cuando:** el endpoint devuelva el arreglo de modalidades en JSON.

### T02. Pruebas y caché

- Verificar que los códigos e iconos coincidan con los requerimientos de UI.
- Comprobar cabeceras `Cache-Control`.

**Archivos:** UnitTest/Transporte/MediosTransporteTests.cs

**Terminado cuando:** las pruebas unitarias validen catálogo completo y headers HTTP.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

