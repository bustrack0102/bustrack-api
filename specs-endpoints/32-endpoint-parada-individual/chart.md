# Diagramas — 32-endpoint-parada-individual — Endpoint de detalle y cercanía de paradas

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/paradas/cercanas?lat=14.59&lng=-90.52] --> B{Validar rangos lat/lng/radio}
  B -->|Inválido| C[400 Bad Request]
  B -->|Válido| D[Consulta geoespacial en Cosmos DB]
  D --> E[Calcular distancia relativa]
  E --> F[Ordenar por distancia ascendente]
  F --> G[200 OK con JSON]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente Flutter] --> PC[ParadasController]
  PC --> PS[ParadaService]
  PS --> DB[(Cosmos DB Paradas Spatial)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente Flutter
  participant A as ParadasController
  participant DB as Persistencia
  C->>A: GET /api/v1/paradas/cercanas?lat=14.59&lng=-90.52
  A->>DB: Query ST_DISTANCE(location, @point) <= @radio
  DB-->>A: Lista de paradas registradas
  A-->>C: 200 OK [ { "nombre": "Parada Central", "distanciaMetros": 120 } ]
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

