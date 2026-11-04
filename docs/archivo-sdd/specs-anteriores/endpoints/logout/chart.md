# Diagramas — Cierre de sesión / revocación

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Usuario] --> B[Logout]
  B --> C[Llamar API]
  C --> D[Limpiar sesión local]
  C -->|falla| D
  D --> E[Redirigir a login]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  C[Cliente] --> API[API]
  API --> R[Blacklist/sesión]
  C --> S[Almacén local]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant U as Usuario
  participant C as Cliente
  participant A as API
  U->>C: logout
  C->>A: POST /auth/logout
  A->>A: revocar token
  A-->>C: 204
  C->>C: limpiar sesión
  C-->>U: login
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
