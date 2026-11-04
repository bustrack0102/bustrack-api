# Diagramas — Endpoint / flujo de login

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Usuario] --> B[Formulario login]
  B --> C[Validación]
  C -->|válido| D[API /auth/login]
  C -->|inválido| E[Error local]
  D -->|200| F[Guardar sesión/token]
  D -->|401/403| G[Credenciales inválidas]
  D -->|429| H[Demasiados intentos]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  U[Cliente] --> API[API central]
  API --> H[(Hash/almacén)]
  API --> J[JWT]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant U as Usuario
  participant C as Cliente
  participant A as API
  U->>C: usuario + contraseña
  C->>A: POST /api/v1/auth/login
  A->>A: validar + verificar hash
  A-->>C: token + expiración
  C-->>U: sesión iniciada
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
