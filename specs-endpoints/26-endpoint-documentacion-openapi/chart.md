# Diagramas — 26-endpoint-documentacion-openapi — Endpoints de documentación OpenAPI y Scalar

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[Petición GET /scalar/v1 o /openapi/v1.json] --> B{¿Es Development?}
  B -->|Sí| C[Generar / Servir OpenAPI o Scalar UI]
  B -->|No| D[404 Not Found]
```

## 2. Componentes

```mermaid
flowchart LR
  Dev[Desarrollador] --> M[Pipeline ASP.NET Core]
  M --> S[Scalar / OpenAPI Engine]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant D as Desarrollador
  participant P as API_SDD
  D->>P: GET /openapi/v1.json
  P-->>D: 200 OK (OpenAPI 3.0 Document)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

