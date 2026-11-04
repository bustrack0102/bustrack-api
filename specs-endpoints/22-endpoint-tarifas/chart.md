# Diagramas — 22-endpoint-tarifas — Endpoint de tarifas

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET tarifas] --> B[Ruta]
  B --> C[(Cosmos)]
  C --> D[Tarifas vigentes]
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
  C->>S: GET /api/v1/rutas/{id}/tarifas
  S-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
