# Diagramas — 31-endpoint-ruta-individual — Endpoint de detalle de ruta

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/rutas/{id}] --> B{¿GUID válido?}
  B -->|No| C[400 Bad Request]
  B -->|Sí| D[Buscar en Cosmos DB Rutas]
  D --> E{¿Encontrada?}
  E -->|No| F[404 Not Found]
  E -->|Sí| G[Mapear RutaDetalleDto con Trazado]
  G --> H[200 OK con JSON]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Flutter / Angular] --> RC[RutasController]
  RC --> RS[RutaService]
  RS --> DB[(Cosmos DB Rutas)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as RutasController
  participant DB as Persistencia
  C->>A: GET /api/v1/rutas/7ee293a8...
  A->>DB: Leer ruta y trazado oficial
  DB-->>A: Registro de ruta
  A-->>C: 200 OK (RutaDetalleDto)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

