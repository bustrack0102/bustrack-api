# Diagramas — 15-endpoint-health — Endpoint de salud

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[Probe] --> B{Chequeo}
  B -->|ok| C[200]
  B -->|falla| D[503]
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
  C->>S: GET /health y /health/ready
  S-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
