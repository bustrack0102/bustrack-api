# Diagramas — 02 — Arquitectura y límites del servicio

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  UI[Presentación/API] --> APP[Aplicación]
  APP --> DOM[Dominio]
  APP --> INF[Infraestructura]
  INF --> DB[(Persistencia)]
  INF --> EXT[Servicios externos]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  C[Clientes] --> API[API central]
  API --> CO[(Cosmos DB)]
  API --> ML[ML]
  API --> G[GenAI]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as API
  participant D as Cosmos
  C->>A: REST
  A->>D: SDK NoSQL
  D-->>A: documento
  A-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
