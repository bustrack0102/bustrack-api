# Diagramas — 17-endpoint-usuario-individual — Endpoint de usuario individual

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET/PUT/DELETE] --> B[AdminPolicy]
  B --> C[Buscar]
  C -->|no| D[404]
  C -->|sí| E[Operación]
  E --> F[Respuesta]
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
  C->>S: GET/PUT/DELETE /api/v1/usuarios/{id}
  S-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
