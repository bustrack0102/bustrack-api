# Diagramas — 05-autenticacion-jwt — Autenticación con JWT

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Login] --> B[Buscar usuario]
  B --> C{¿Bloqueado?}
  C -->|sí| D[429/423]
  C -->|no| E[Verificar Argon2id]
  E -->|ok| F[Emitir JWT]
  E -->|falla| G[Registrar intento]
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

JWT HS256; no hay refresh token ni 2FA.
