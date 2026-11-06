# Tareas — 27-endpoint-empresa-individual — Endpoint de detalle de empresa de transporte

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Definir contrato y DTO de detalle

- Especificar esquema de respuesta `EmpresaDetalleDto`.
- Definir parámetros y rutas en el controlador.

**Archivos:** API_SDD/Dtos/Transporte/EmpresaDetalleDto.cs, API_SDD/Controllers/V1/EmpresasController.cs

**Terminado cuando:** el DTO y la signatura del método `GetByIdAsync` estén definidos y compilando.

### T02. Pruebas de contrato y servicio

- Crear casos de prueba para id existente (200), inexistente (404) e inválido (400).
- Verificar que no expone datos no autorizados.

**Archivos:** UnitTest/Transporte/EmpresasTests.cs

**Terminado cuando:** la suite de pruebas cubra los tres escenarios esperados.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

