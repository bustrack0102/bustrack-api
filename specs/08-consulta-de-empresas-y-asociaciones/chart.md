# Diagramas — 08-consulta-de-empresas-y-asociaciones — Consulta de empresas y asociaciones

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Cliente] --> B[GET empresas]
  B --> C[Filtrar/paginar]
  C --> D[(Cosmos)]
  D --> E[DTO]
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

Diagramas de proceso; no representan recursos desplegados.
