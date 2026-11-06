# Diagramas — 36-endpoint-ml-inferencia — Endpoint de inferencia de telemetría y calidad vial (ML)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/telemetria/inferencia] --> B{Validar muestras <= 500}
  B -->|Excedido / Inválido| C[400 / 422 Error]
  B -->|Válido| D[Llamar cliente interno ML]
  D --> E{¿Respuesta ML en < 2s?}
  E -->|Timeout / Caída| F[502 Bad Gateway / 504 Timeout]
  E -->|OK 200| G[Mapear InferenciaResponseDto]
  G --> H[200 OK con JSON]
```

## 2. Componentes

```mermaid
flowchart LR
  F[Cliente Móvil Flutter] --> TC[TelemetriaController]
  TC --> MC[MlServiceClient]
  MC --> ML[(FastAPI ML Service Interno)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant F as Flutter
  participant API as API Central
  participant ML as FastAPI ML Interno
  F->>API: POST /api/v1/telemetria/inferencia (lote)
  API->>ML: POST /inference (auth interno)
  ML-->>API: 200 { "class": "BACHE_SEVERO", "conf": 0.94 }
  API-->>F: 200 OK (InferenciaResponseDto)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

