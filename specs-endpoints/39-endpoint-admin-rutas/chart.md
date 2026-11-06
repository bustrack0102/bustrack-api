# Diagramas — 39-endpoint-admin-rutas — Endpoint de administración de rutas y paradas

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/rutas] --> B{¿AdminPolicy válida?}
  B -->|No| C[401 / 403 Error]
  B -->|Sí| D[Validar código único y paradas]
  D -->|Código existe| E[409 Conflict]
  D -->|Parada no existe| F[404 Not Found]
  D -->|Válido| G[Guardar ruta en Cosmos DB Rutas]
  G --> H[201 Created con Location]
```

## 2. Componentes

```mermaid
flowchart LR
  Admin[Panel Administrativo] --> RC[RutasController]
  RC --> V[CrearRutaValidator]
  RC --> RAS[RutaAdminService]
  RAS --> DB[(Cosmos DB)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant Admin as Panel Admin
  participant API as RutasController
  participant DB as Persistencia
  Admin->>API: POST /api/v1/rutas (Datos + Trazado GeoJSON)
  API->>DB: Validar que no exista código
  API->>DB: Insertar registro de ruta
  DB-->>API: Confirmación de persistencia
  API-->>Admin: 201 Created
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

