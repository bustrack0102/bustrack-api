# Diagramas — 16-endpoint-usuarios-coleccion — Endpoint de colección de usuarios

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[GET/POST] --> B[AdminPolicy]
  B --> C[Validar]
  C --> D[(SQL Server)]
  D --> E[Respuesta]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Consumidor] --> A[Servicio]
  A --> D[(Datos)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Consumidor
  participant S as Servicio
  C->>S: GET/POST /api/v1/usuarios
  S-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
