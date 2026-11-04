# Diagramas — 06-gestion-de-sesiones-y-logout — Gestión de sesiones y logout

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Logout] --> B[Validar JWT]
  B --> C[Agregar jti a blacklist]
  C --> D[204]
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

La blacklist en memoria es una limitación conocida; no hay Redis.
