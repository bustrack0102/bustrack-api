# Tareas — 25-endpoint-probes — Endpoints de diagnóstico y sondeo de errores

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.

## Actividades

### T01. Verificar rutas y respuestas

- Probar `/timeout`, `/error`, `/forbidden`, `/conflict`, `/invalid`.
- Verificar formato RFC 7807 ProblemDetails.

**Archivos:** API_SDD/Controllers/V1/ProbeController.cs

**Terminado cuando:** cada ruta responde con el código simulado correspondiente.

### T02. Probar restricción de producción

- Configurar `ASPNETCORE_ENVIRONMENT=Production` y ejecutar peticiones.
- Confirmar retorno `404 Not Found`.

**Archivos:** IntegrationTest/

**Terminado cuando:** ninguna sonda responde en entorno productivo.

## Cierre

- [x] Contrato verificado contra el código.
- [x] Pruebas de éxito y error.
- [x] Documentación de consumidores actualizada.
- [x] Sin secretos ni PII en ejemplos.

