# Tareas — 37-endpoint-genai-recomendaciones — Endpoint de recomendaciones y planificación de viajes (GenAI)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTOs y Cliente GenAI

- Diseñar `RecomendacionRequestDto` y `RecomendacionResponseDto`.
- Definir cliente `IGenAiServiceClient` para conectar con el servicio interno.

**Archivos:** API_SDD/Dtos/GenAi/RecomendacionRequestDto.cs, API_SDD/Services/GenAi/IGenAiServiceClient.cs

**Terminado cuando:** los DTOs y la interfaz del cliente de IA estén definidos.

### T02. Pruebas de orquestación y blindaje contra alucinación

- Verificar que solo se sugieran rutas presentes en la respuesta de la base de datos.
- Probar manejo de fallback si el servicio GenAI no responde.

**Archivos:** UnitTest/GenAi/RecomendacionesTests.cs

**Terminado cuando:** las pruebas unitarias validen que la respuesta no contiene información no verificada.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

