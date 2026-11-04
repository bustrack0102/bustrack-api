# Diagramas — 12-migracion-de-persistencia-a-cosmos-db — Migración de persistencia a Cosmos DB for NoSQL

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[SQL Server] --> B[Mapeo documental]
  B --> C[Migración por fases]
  C --> D[Validar]
  D -->|ok| E[Cosmos]
  D -->|falla| F[Rollback]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente] --> A[Servicio]
  A --> D[(Datos)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant D as Dev
  participant C as CI
  D->>C: push
  C-->>D: lint + pruebas
```

## 4. Notas

No se ha ejecutado ninguna migración.
