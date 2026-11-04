# Diagramas — 07-modelo-de-dominio-de-transporte — Modelo de dominio de transporte

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart LR
  E[Empresa] --> R[Ruta]
  R --> P[Parada]
  R --> H[Horario]
  R --> T[Tarifa]
  U[Usuario] --> C[Calificación]
  C --> E
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
