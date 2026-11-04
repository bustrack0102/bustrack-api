# Diagramas — 14-endpoint-logout — Endpoint de logout

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST logout] --> B[Validar JWT]
  B -->|válido| C[Blacklist jti]
  C --> D[204]
  B -->|inválido| E[401]
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
  C->>S: POST /api/v1/auth/logout
  S-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
