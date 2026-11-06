# Diagramas — 42-endpoint-auth-recuperar-password — Endpoint de solicitud de recuperación de contraseña (EP-AU-05)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[POST /api/v1/auth/recuperar-password] --> B{¿Formato correo válido?}
  B -->|No| C[400 Bad Request]
  B -->|Sí| D[Buscar usuario por correo normalizado]
  D --> E{¿Usuario existe y activo?}
  E -->|No| F[Responder 200 genérico]
  E -->|Sí| G[Invalidar tokens previos pendientes]
  G --> H[Generar token criptográfico vigencia 30m]
  H --> I[Guardar hash de token en tokens_recuperacion]
  I --> J[Enviar correo con enlace]
  J --> F[Responder 200 genérico]
```

## 2. Componentes

```mermaid
flowchart LR
  App[Cliente Móvil / Web] --> AC[AuthController]
  AC --> RS[RecuperacionPasswordService]
  RS --> DB[(Persistencia)]
  RS --> ES[EmailSender]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant App as Cliente
  participant API as AuthController
  participant S as RecuperacionService
  participant DB as Persistencia
  participant E as Servicio Correo
  App->>API: POST /api/v1/auth/recuperar-password { "correo": "..." }
  API->>S: SolicitarRecuperacionAsync(correo)
  S->>DB: Consultar usuario
  opt Usuario existe
    S->>DB: Invalidar tokens previos y guardar hash de nuevo token
    S->>E: Enviar correo con enlace y token
  end
  API-->>App: 200 OK { "mensaje": "Si el correo está registrado..." }
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
