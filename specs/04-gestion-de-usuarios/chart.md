# Diagramas — 04-gestion-de-usuarios — Gestión de usuarios

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Admin] --> B[CRUD usuarios]
  B --> C[Validar]
  C --> D[Hashear]
  D --> E[(SQL Server)]
  E --> F[Respuesta]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente] --> A[Servicio]
  A --> D[(Datos)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant D as Dev
  participant C as CI
  D->>C: push
  C-->>D: lint + pruebas
```

## 4. Notas

La persistencia cambiará a Cosmos DB en la tarea 12.
