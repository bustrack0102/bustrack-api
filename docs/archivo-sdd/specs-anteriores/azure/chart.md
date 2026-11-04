# Diagramas — Arquitectura Azure propuesta

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart LR
  U[Usuarios] --> W[Static Web Apps/Angular]
  U --> M[Flutter]
  W --> API[API App Service]
  M --> API
  API --> C[(Cosmos DB)]
  API --> ML[ML Container]
  API --> G[GenAI Container]
  API --> KV[Key Vault]
  API --> AI[App Insights]
```

## 2. Relación entre componentes

```mermaid
flowchart TD
  API[API] --> KV[Key Vault]
  API --> DB[(Cosmos)]
  ML[ML] --> KV
  G[GenAI] --> KV
  API --> AI[App Insights]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant W as Frontend
  participant A as API
  participant K as Key Vault
  participant D as Cosmos
  C->>W: navegación
  W->>A: HTTPS
  A->>K: secreto (identidad)
  A->>D: consulta
  D-->>A: datos
  A-->>W: respuesta
```

## 4. Notas

Todos los recursos son propuesta; no hay suscripción, grupo de recursos ni despliegue verificados.
