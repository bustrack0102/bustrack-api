# Diagramas — 40-endpoint-admin-horarios-tarifas — Endpoint de administración de horarios y tarifas de ruta

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/rutas/{id}/horarios] --> B{¿AdminPolicy?}
  B -->|No| C[401 / 403 Error]
  B -->|Sí| D[Comprobar existencia de ruta]
  D -->|Ruta inexistente| E[404 Not Found]
  D -->|Existe| F{¿Hay solapamiento horario?}
  F -->|Sí| G[409 Conflict]
  F -->|No| H[Guardar horario]
  H --> I[201 Created]
```

## 2. Componentes

```mermaid
flowchart LR
  Admin[Panel Admin] --> HC[RutaHorariosController]
  HC --> HV[CrearHorarioValidator]
  HC --> HAS[HorarioAdminService]
  HAS --> DB[(Cosmos DB)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant Admin as Panel Admin
  participant API as RutaHorariosController
  participant DB as Persistencia
  Admin->>API: POST /api/v1/rutas/7ee.../horarios (Horario)
  API->>DB: Validar existencia de ruta y solapamientos
  DB-->>API: Validación exitosa
  API->>DB: Guardar bloque horario
  API-->>Admin: 201 Created
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

