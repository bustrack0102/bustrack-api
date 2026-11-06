# Plan técnico — 37-endpoint-genai-recomendaciones — Endpoint de recomendaciones y planificación de viajes (GenAI)

> Spec: `spec.md` · Endpoint: `POST /api/v1/recomendaciones` · Estado: Propuesto.

## 1. Estrategia

Implementar un flujo de dos fases (Retrieval-Augmented Generation / RAG acotado):
1. La API consulta rutas, paradas y horarios reales en Cosmos DB según el origen y destino solicitados.
2. La API envía ese contexto validado al servicio interno de GenAI (OpenAI API / FastAPI interno) para redactar la explicación estructurada.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, HttpClientFactory, Polly.
- Componentes: `RecomendacionesController`, `IGenAiServiceClient`, `IRutaService`, `RecomendacionRequestDto`, `RecomendacionResponseDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/RecomendacionesController.cs
API_SDD/Dtos/GenAi/RecomendacionRequestDto.cs
API_SDD/Dtos/GenAi/RecomendacionResponseDto.cs
API_SDD/Services/GenAi/IGenAiServiceClient.cs
API_SDD/Services/GenAi/GenAiServiceClient.cs
```

## 4. Seguridad

- ASVS L2, sanitización de entrada para neutralizar inyecciones de prompt.
- Secretos de OpenAI / LLM gestionados estrictamente en variables de entorno o Key Vault; cero secretos en Git.

## 5. Errores

- Retorno RFC 7807 uniforme. En caso de timeout de IA, devolver respuesta en modo degradado basada únicamente en las rutas encontradas en la base de datos sin explicación generativa.

## 6. Pruebas

- Pruebas unitarias de sanitización de preguntas libres.
- Pruebas de integración simulando el servicio GenAI con mocks.

## 7. Riesgos

- Alucinación de datos mitigada por inyección de contexto cerrado con instrucción del sistema "Responde únicamente con los datos provistos".

