# Diagramas — 11-gestion-de-calificaciones — Gestión de calificaciones

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Usuario] --> B[POST calificación]
  B --> C[Validar + autorizar]
  C --> D[(Cosmos)]
  D --> E[Agregado]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Cliente] --> A[Servicio]
  A --> D[(Datos)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant D as Dev
  participant C as CI
  D->>C: push
  C-->>D: lint + pruebas
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
