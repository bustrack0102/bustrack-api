# Diagramas — 29-endpoint-destinos — Endpoint de destinos y terminales

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/destinos?municipio=VillaNueva] --> B[Validar query params]
  B --> C[Consultar repositorio de destinos]
  C --> D[Mapear a DestinoDto]
  D --> E[200 OK con PagedResult]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Flutter / Angular] --> DC[DestinosController]
  DC --> DS[DestinoService]
  DS --> DB[(Cosmos DB Destinos)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant API as DestinosController
  participant DB as Persistencia
  C->>API: GET /api/v1/destinos
  API->>DB: Consultar destinos activos
  DB-->>API: Colección de terminales y destinos
  API-->>C: 200 OK (PagedResult<DestinoDto>)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

