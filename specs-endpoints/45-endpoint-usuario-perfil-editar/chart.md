# Diagramas — 45-endpoint-usuario-perfil-editar — Endpoint de edición de perfil de usuario autenticado (EP-AU-08)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[PUT /api/v1/usuarios/me con Bearer JWT] --> B{¿Token válido?}
  B -->|No| C[401 Unauthorized]
  B -->|Sí| D[Extraer userId del claim sub]
  D --> E{¿Cambia correo?}
  E -->|Sí| F{¿Correo ya registrado por otro id?}
  F -->|Sí| G[409 Conflict]
  F -->|No| H{¿Cambia password?}
  E -->|No| H
  H -->|Sí| I{¿passwordActual válida?}
  I -->|No| J[400 Bad Request]
  I -->|Sí| K[Generar nuevo hash Argon2id]
  H -->|No| L[Actualizar datos en BD]
  K --> L
  L --> M[200 OK con PerfilActualizadoResponseDto]
```

## 2. Componentes

```mermaid
flowchart LR
  App[Flutter / Angular] --> UC[UsuariosController]
  UC --> V[EditarPerfilValidator]
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
  App->>API: PUT /api/v1/usuarios/me (cambios)
  API->>S: ActualizarPerfilAsync(userId, dto)
  S->>DB: Validar correo único
  opt Incluye nueva contraseña
    S->>S: Verificar passwordActual con hash
    S->>S: Generar hash nuevo Argon2id
  end
  S->>DB: Guardar cambios
  API-->>App: 200 OK (PerfilActualizadoResponseDto)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
