# Diagramas — 33-endpoint-auth-refresh — Endpoint de refresco y renovación de tokens JWT

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/auth/refresh] --> B{¿Body válido?}
  B -->|No| C[400 Bad Request]
  B -->|Sí| D[Buscar hash de RefreshToken]
  D --> E{¿Válido y vigente?}
  E -->|No / Ya usado| F[401 Unauthorized + Revocar familia]
  E -->|Sí| G[Generar nuevo JWT HS256]
  G --> H[Rotar y guardar nuevo RefreshToken]
  H --> I[200 OK con nuevos tokens]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Flutter / Angular] --> AC[AuthController]
  AC --> TS[TokenService]
  TS --> DB[(Persistencia Tokens)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as AuthController
  participant T as TokenService
  C->>A: POST /api/v1/auth/refresh (expiredJwt, refreshToken)
  A->>T: Validar y Rotar
  T-->>A: Nuevo par (jwt, rotadoRefreshToken)
  A-->>C: 200 OK
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

