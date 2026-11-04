# Diagramas — Migración SQL Server/EF Core → Azure Cosmos DB for NoSQL

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[SQL Server] --> B[Inventario de datos]
  B --> C[Mapeo a documentos]
  C --> D[Migración por fases]
  D --> E[Validación]
  E -->|ok| F[Cosmos DB]
  E -->|falla| G[Rollback]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  API[API] --> R[Repositorio]
  R --> SQL[(SQL Server actual)]
  R -.-> CO[(Cosmos objetivo)]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant A as API
  participant R as Repositorio
  participant S as SQL
  participant C as Cosmos
  A->>R: operación
  R->>S: actual
  S-->>R: resultado
  Note over R,C: fase futura
  R->>C: escritura espejo
  C-->>R: ETag
```

## 4. Notas

Muestra estado actual y objetivo; no se ha ejecutado migración.
