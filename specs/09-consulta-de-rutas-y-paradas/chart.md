# Diagramas — 09-consulta-de-rutas-y-paradas — Consulta de rutas y paradas

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Cliente] --> B[GET rutas]
  B --> C[Filtrar]
  C --> D[(Cosmos)]
  D --> E[Ruta + paradas ordenadas]
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
