# Diagramas — 37-endpoint-genai-recomendaciones — Endpoint de recomendaciones y planificación de viajes (GenAI)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/recomendaciones] --> B[Sanitizar input de usuario]
  B --> C[Consultar rutas reales en Cosmos DB]
  C --> D{¿Se encontraron rutas?}
  D -->|No| E[Responder con rutas no encontradas]
  D -->|Sí| F[Armar contexto estricto y enviar a GenAI interno]
  F --> G{¿Responde GenAI en tiempo?}
  G -->|Timeout / Falla| H[Fallback: devolver rutas reales sin texto generado]
  G -->|OK 200| I[200 OK con Rutas + Explicación]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Flutter / Angular] --> RC[RecomendacionesController]
  RC --> RS[RutaService]
  RS --> DB[(Cosmos DB)]
  RC --> GC[GenAiServiceClient]
  GC --> AI[(FastAPI GenAI Interno)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant API as RecomendacionesController
  participant DB as Cosmos DB
  participant AI as GenAI Interno
  C->>API: POST /api/v1/recomendaciones (origen, destino)
  API->>DB: Buscar rutas coincidentes
  DB-->>API: Rutas oficiales R-101
  API->>AI: POST /recommendations (contexto de rutas oficiales)
  AI-->>API: Explicación y tiempos
  API-->>C: 200 OK (RecomendacionResponseDto)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

