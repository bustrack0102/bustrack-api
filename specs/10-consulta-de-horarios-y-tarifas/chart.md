# Diagramas — 10-consulta-de-horarios-y-tarifas — Consulta de horarios y tarifas

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Ruta] --> B[Horarios]
  A --> C[Tarifas]
  B --> D[DTO]
  C --> D
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
