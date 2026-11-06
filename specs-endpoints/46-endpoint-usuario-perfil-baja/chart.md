# Diagramas — 46-endpoint-usuario-perfil-baja — Endpoint de baja de cuenta de usuario autenticado (EP-AU-09)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[DELETE /api/v1/usuarios/me con Bearer JWT] --> B{¿Token válido?}
  B -->|No| C[401 Unauthorized]
  B -->|Sí| D[Obtener userId del claim sub]
  D --> E{¿passwordConfirmacion coincide con hash?}
  E -->|No| F[400 Bad Request]
  E -->|Sí| G[Iniciar Transacción de Baja]
  G --> H[Marcar activo = false]
  H --> I[Anonimizar nombre y correo]
  I --> J[Eliminar refresh_tokens y viajes_guardados]
  J --> K[Preservar calificaciones desvinculadas]
  K --> L[Confirmar Transacción]
  L --> M[200 OK con mensaje de confirmación]
```

## 2. Componentes

```mermaid
flowchart LR
  App[Flutter / Angular] --> UC[UsuariosController]
  UC --> V[BajaCuentaValidator]
  UC --> US[UsuarioService]
  US --> PH[Argon2idHasher]
  US --> DB[(Persistencia)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant App as Cliente
  participant API as UsuariosController
  participant S as UsuarioService
  participant DB as Persistencia
  App->>API: DELETE /api/v1/usuarios/me { "passwordConfirmacion": "..." }
  API->>S: DarDeBajaCuentaAsync(userId, password)
  S->>DB: Leer hash de usuario
  S->>S: Verificar contraseña
  S->>DB: Anonimizar perfil y revocar refresh tokens
  DB-->>S: Transacción exitosa
  API-->>App: 200 OK { "mensaje": "Tu cuenta y datos personales..." }
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
