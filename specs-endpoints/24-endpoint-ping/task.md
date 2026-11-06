# Tareas — 24-endpoint-ping — Endpoint de ping

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.

## Actividades

### T01. Verificar contrato y controlador

- Validar ruta `/api/v1/ping`.
- Comprobar serialización y headers.

**Archivos:** API_SDD/Controllers/V1/PingController.cs

**Terminado cuando:** el controlador devuelve 200 OK con modelo JSON válido.

### T02. Pruebas y cobertura

- Prueba de integración contra WebApplicationFactory.
- Verificación de cabeceras de correlación y rate limit.

**Archivos:** IntegrationTest/

**Terminado cuando:** las pruebas HTTP confirman código 200 y headers esperados.

## Cierre

- [x] Contrato verificado contra el código.
- [x] Pruebas de éxito y error.
- [x] Documentación de consumidores actualizada.
- [x] Sin secretos ni PII en ejemplos.

