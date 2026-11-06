# Tareas — 36-endpoint-ml-inferencia — Endpoint de inferencia de telemetría y calidad vial (ML)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTOs y Cliente HTTP de ML

- Crear `LoteTelemetriaRequestDto` con validaciones de rango numérico.
- Diseñar interfaz `IMlServiceClient` y cliente tipado con timeout estricto.

**Archivos:** API_SDD/Dtos/Ml/LoteTelemetriaRequestDto.cs, API_SDD/Services/Ml/IMlServiceClient.cs

**Terminado cuando:** el DTO valide las muestras y la interfaz esté registrada en el contenedor DI.

### T02. Pruebas de orquestación y fallos

- Probar forwarding exitoso de lote y mapeo de `InferenciaResponseDto`.
- Probar respuesta controlada 502/504 ante caída del servicio ML interno.

**Archivos:** UnitTest/Ml/InferenciaTests.cs

**Terminado cuando:** las pruebas unitarias y mocks demuestren tolerancia a fallos.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

