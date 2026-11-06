# Diagramas — 28-endpoint-asociaciones — Endpoint de cooperativas y asociaciones de transporte

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/asociaciones?page=1&pageSize=10] --> B{Validar paginación}
  B -->|Inválido| C[400 Bad Request]
  B -->|Válido| D[Consultar contenedor Cosmos Asociaciones]
  D --> E[Armar PagedResult]
  E --> F[200 OK con JSON]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente] --> AC[AsociacionesController]
  AC --> AS[AsociacionService]
  AS --> DB[(Cosmos DB)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as AsociacionesController
  participant S as AsociacionService
  C->>A: GET /api/v1/asociaciones
  A->>S: ObtenerPaginado(page, pageSize)
  S-->>A: PagedResult<AsociacionDto>
  A-->>C: 200 OK
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

