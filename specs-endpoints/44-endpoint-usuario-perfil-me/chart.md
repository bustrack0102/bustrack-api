# Diagramas — 44-endpoint-usuario-perfil-me — Endpoint de consulta de perfil de usuario autenticado (EP-AU-07)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET /api/v1/usuarios/me con Bearer JWT] --> B{¿Token válido y no revocado?}
  B -->|No| C[401 Unauthorized]
  B -->|Sí| D[Extraer userId del claim sub]
  D --> E[Buscar usuario por ID]
  E --> F{¿Existe y activo?}
  F -->|No| G[404 Not Found]
  F -->|Sí| H[Cargar conteos de viajes, calificaciones y estado de sensores]
  H --> I[200 OK con UsuarioPerfilDto]
```

## 2. Componentes

```mermaid
flowchart LR
  App[Flutter / Angular] --> UC[UsuariosController]
  UC --> US[UsuarioService]
  US --> DB[(Persistencia)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant App as Cliente
  participant API as UsuariosController
  participant S as UsuarioService
  participant DB as Persistencia
  App->>API: GET /api/v1/usuarios/me (Bearer Token)
  API->>S: ObtenerPerfilAsync(userId)
  S->>DB: Leer usuario + rol + consentimientos + conteos
  DB-->>S: Datos consolidados
  API-->>App: 200 OK (UsuarioPerfilDto)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
