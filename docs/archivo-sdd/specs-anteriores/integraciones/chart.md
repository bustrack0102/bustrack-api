# Diagramas — Integraciones con el ecosistema BusTrack

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart LR
  M[Flutter] --> API[API]
  W[Angular] --> API
  API --> DB[(Cosmos)]
  API <--> ML[ML]
  API <--> G[GenAI]
```

## 2. Relación entre componentes

```mermaid
flowchart TD
  C[Cliente] -->|HTTPS| API
  API -->|SDK| DB
  API -->|HTTP interno| S[ML/GenAI]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as API
  participant S as Servicio
  C->>A: request
  A->>S: contexto validado
  S-->>A: resultado
  A-->>C: respuesta
```

## 4. Notas

Solo login/logout están implementados hoy; el resto es propuesta.
