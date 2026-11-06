# Diagramas — 38-endpoint-admin-empresas — Endpoint de administración de empresas de transporte

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[Petición Admin POST/PUT/DELETE /api/v1/empresas] --> B{¿JWT válido y Rol Admin?}
  B -->|No| C[401 Unauthorized / 403 Forbidden]
  B -->|Sí| D[Validar DTO con FluentValidation]
  D -->|Inválido| E[422 Unprocessable Entity]
  D -->|Válido| F{¿Operación?}
  F -->|POST| G[Insertar empresa -> 201 Created]
  F -->|PUT| H[Comprobar ETag / Actualizar -> 200 OK]
  F -->|DELETE| I[Marcar activo = false -> 204 No Content]
```

## 2. Componentes

```mermaid
flowchart LR
  Admin[Angular Admin Panel] --> EC[EmpresasController]
  EC --> V[CrearEmpresaValidator]
  EC --> EAS[EmpresaAdminService]
  EAS --> DB[(Cosmos DB)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant Admin as Panel Admin
  participant API as EmpresasController
  participant DB as Persistencia
  Admin->>API: POST /api/v1/empresas (Bearer AdminToken)
  API->>API: Validar AdminPolicy y esquema
  API->>DB: Guardar nueva empresa
  DB-->>API: Empresa creada (Id generado)
  API-->>Admin: 201 Created { Location: /api/v1/empresas/{id} }
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

