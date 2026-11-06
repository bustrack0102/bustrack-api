# Tareas — 26-endpoint-documentacion-openapi — Endpoints de documentación OpenAPI y Scalar

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.

## Actividades

### T01. Verificar generación de esquema

- Consultar `/openapi/v1.json` en modo Development.
- Validar JSON válido con información de DTOs y respuestas.

**Archivos:** API_SDD/Program.cs, API_SDD/Hosting/

**Terminado cuando:** el documento OpenAPI es devuelto con status 200 y sintaxis correcta.

### T02. Verificar restricción de entorno

- Simular entorno de Producción.
- Validar código 404 en ambas rutas.

**Archivos:** IntegrationTest/

**Terminado cuando:** las rutas devuelven 404 fuera de Development.

## Cierre

- [x] Contrato verificado contra el código.
- [x] Pruebas de éxito y error.
- [x] Documentación de consumidores actualizada.
- [x] Sin secretos ni PII en ejemplos.

