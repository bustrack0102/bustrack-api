# Diagramas — 43-endpoint-auth-restablecer-password — Endpoint de restablecimiento de contraseña (EP-AU-06)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/auth/restablecer-password] --> B{Validar coincidencia de password y token}
  B -->|Inválido| C[400 Bad Request]
  B -->|Válido| D[Buscar hash de token en tokens_recuperacion]
  D --> E{¿Existe, usado == false y no expirado?}
  E -->|No| F[400 Bad Request]
  E -->|Sí| G[Calcular hash Argon2id de nueva contraseña]
  G --> H[Iniciar Transacción]
  H --> I[Actualizar hash de contraseña en usuarios]
  I --> J[Marcar token como usado]
  J --> K[Revocar todos los Refresh Tokens del usuario]
  K --> L[Confirmar Transacción]
  L --> M[200 OK]
```

## 2. Componentes

```mermaid
flowchart LR
  App[Flutter / Angular] --> AC[AuthController]
  AC --> V[RestablecerPasswordValidator]
  AC --> RS[RestablecerPasswordService]
  RS --> PH[Argon2idHasher]
  RS --> DB[(Persistencia)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant App as Cliente
  participant API as AuthController
  participant S as RestablecerService
  participant DB as Persistencia
  App->>API: POST /api/v1/auth/restablecer-password (token, passwords)
  API->>S: RestablecerPasswordAsync(dto)
  S->>DB: Validar token de recuperación
  DB-->>S: Token vigente
  S->>DB: Actualizar contraseña, quemar token y revocar sesiones
  DB-->>S: Transacción exitosa
  API-->>App: 200 OK { "mensaje": "Contraseña actualizada..." }
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
