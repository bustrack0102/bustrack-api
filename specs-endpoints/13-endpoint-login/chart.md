# Diagramas — 13-endpoint-login — Endpoint de login

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST login] --> B[Validar DTO]
  B --> C[Buscar usuario]
  C --> D{Lockout}
  D -->|sí| E[429/423]
  D -->|no| F[Argon2id]
  F -->|ok| G[JWT]
  F -->|falla| H[401]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Consumidor] --> A[Servicio]
  A --> D[(Datos)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Consumidor
  participant S as Servicio
  C->>S: POST /api/v1/auth/login
  S-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
