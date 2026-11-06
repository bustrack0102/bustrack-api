# Diagramas — 35-endpoint-admin-bootstrap — Endpoint de bootstrap inicial de administrador

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/admin/bootstrap] --> B{¿X-Bootstrap-Key válida?}
  B -->|No| C[401 Unauthorized]
  B -->|Sí| D{¿Existen administradores en DB?}
  D -->|Sí, count > 0| E[409 Conflict - Sistema ya inicializado]
  D -->|No, count == 0| F[Validar DTO y hash Argon2id]
  F --> G[Insertar usuario administrador]
  G --> H[201 Created]
```

## 2. Componentes

```mermaid
flowchart LR
  Deploy[Script Despliegue / DevOps] --> BC[BootstrapController]
  BC --> BS[BootstrapService]
  BS --> DB[(Cosmos DB Usuarios)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant D as DevOps / CLI
  participant B as BootstrapController
  participant S as BootstrapService
  participant DB as Persistencia
  D->>B: POST /api/v1/admin/bootstrap (Key + Datos)
  B->>S: EjecutarBootstrapAsync(dto)
  S->>DB: Contar usuarios con rol Admin
  DB-->>S: 0
  S->>DB: Crear usuario inicial
  B-->>D: 201 Created
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

