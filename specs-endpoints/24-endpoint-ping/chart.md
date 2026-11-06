# Diagramas — 24-endpoint-ping — Endpoint de ping

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[Cliente envía GET /api/v1/ping] --> B[Middleware de pipeline / headers]
  B --> C[PingController]
  C --> D[Generar timestamp UTC y correlation ID]
  D --> E[200 OK con JSON]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente Flutter / Angular / Monitor] --> P[Pipeline HTTP API_SDD]
  P --> K[PingController]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant S as API_SDD
  C->>S: GET /api/v1/ping
  S-->>C: 200 OK { "status": "ok", "timestamp": "..." }
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

