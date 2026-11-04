# Diagramas — 02-arquitectura-base-y-pipeline-http — Arquitectura base y pipeline HTTP

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Request] --> B[Correlación]
  B --> C[Rate limit]
  C --> D[Auth]
  D --> E[Endpoint]
  E --> F[Headers]
  F --> G[Response]
  E -.error.-> H[Exception middleware]
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

Pipeline implementado; no implica despliegue.
