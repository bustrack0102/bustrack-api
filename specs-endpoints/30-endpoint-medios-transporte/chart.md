# Diagramas — 30-endpoint-medios-transporte — Endpoint de medios y modalidades de transporte

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/medios-transporte] --> B{¿Está en caché?}
  B -->|Sí| C[Retornar desde IMemoryCache]
  B -->|No| D[Consultar base de datos]
  D --> E[Almacenar en caché 1h]
  E --> F[200 OK]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente] --> MC[MediosTransporteController]
  MC --> MS[MedioTransporteService]
  MS --> Cache[IMemoryCache]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as API
  participant Cache as Memoria Caché
  C->>A: GET /api/v1/medios-transporte
  A->>Cache: Obtener catálogo
  Cache-->>A: Lista en memoria
  A-->>C: 200 OK [ { "codigo": "BUS_URBANO", ... } ]
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

