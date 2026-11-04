# Diagramas — 23-endpoint-calificaciones — Endpoint de calificaciones

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[Usuario] --> B[POST calificación]
  B --> C[Validar + autorizar]
  C --> D[(Cosmos)]
  D --> E[Agregado]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Consumidor] --> A[Servicio]
  A --> D[(Datos)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Consumidor
  participant S as Servicio
  C->>S: GET/POST /api/v1/calificaciones
  S-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
