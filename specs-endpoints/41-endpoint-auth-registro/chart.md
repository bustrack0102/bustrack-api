# Diagramas — 41-endpoint-auth-registro — Endpoint de registro de usuario (EP-AU-01)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/auth/registro] --> B{Validar campos y privacidad == true}
  B -->|No| C[400 Bad Request]
  B -->|Sí| D{¿Existe correo en minúsculas?}
  D -->|Sí| E[409 Conflict]
  D -->|No| F[Generar hash Argon2id de password]
  F --> G[Insertar usuario con rol Usuario]
  G --> H[Generar AccessToken JWT + RefreshToken]
  H --> I[Guardar hash de RefreshToken]
  I --> J[201 Created con usuario y tokens]
```

## 2. Componentes

```mermaid
flowchart LR
  App[Flutter / Angular] --> AC[AuthController]
  AC --> V[RegistroValidator]
  AC --> AS[AuthService]
  AS --> PH[Argon2idHasher]
  AS --> TS[JwtTokenService]
  AS --> DB[(Persistencia Usuarios)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant App as App Móvil
  participant API as AuthController
  participant S as AuthService
  participant DB as Persistencia
  App->>API: POST /api/v1/auth/registro (datos, privacidad: true)
  API->>S: RegistroAsync(dto)
  S->>DB: Verificar unicidad de correo
  DB-->>S: No existe duplicado
  S->>DB: Insertar usuario y hash de refresh token
  S-->>API: Resultado con tokens
  API-->>App: 201 Created (usuario, accessToken, refreshToken)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
