# Diagramas — 27-endpoint-empresa-individual — Endpoint de detalle de empresa de transporte

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/empresas/{id}] --> B{¿GUID válido?}
  B -->|No| C[400 Bad Request]
  B -->|Sí| D[Consultar servicio de empresas]
  D --> E{¿Existe empresa?}
  E -->|No| F[404 Not Found]
  E -->|Sí| G[Mapear EmpresaDetalleDto]
  G --> H[200 OK con JSON]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente Flutter / Angular] --> EC[EmpresasController]
  EC --> ES[EmpresaService]
  ES --> DB[(Cosmos DB / SQL)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant API as EmpresasController
  participant DB as Persistencia
  C->>API: GET /api/v1/empresas/3fa85f64...
  API->>DB: Obtener por Id
  DB-->>API: Entidad encontrada
  API-->>C: 200 OK (EmpresaDetalleDto)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

