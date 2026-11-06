# Diagramas — 25-endpoint-probes — Endpoints de diagnóstico y sondeo de errores

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/probe/{tipo}] --> B{¿Es Producción?}
  B -->|Sí| C[404 Not Found]
  B -->|No| D{Tipo de sonda}
  D -->|timeout| E[408 Request Timeout]
  D -->|error| F[500 Internal Error]
  D -->|forbidden| G[403 Forbidden]
  D -->|conflict| H[409 Conflict]
  D -->|invalid| I[422 Unprocessable]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente / Tests] --> P[ProbeController]
  P --> E[Exception / Status Generator]
  E --> M[ProblemDetails Middleware]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente de Test
  participant P as ProbeController
  participant M as RFC 7807 Middleware
  C->>P: GET /api/v1/probe/invalid
  P->>M: Disparar excepción / validación
  M-->>C: 422 Unprocessable Entity { "title": "Validation Error", ... }
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

