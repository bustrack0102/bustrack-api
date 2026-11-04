# Diagramas — 00 — Fundamentos y gobernanza SDD

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Leer constitution y Memoria] --> B[Elegir spec]
  B --> C[Revisar plan y tareas]
  C --> D[Implementar + probar]
  D --> E[Actualizar Memoria]
  E --> F[Revisar PR]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  G[Gobernanza] --> S[Specs]
  S --> C[Código]
  C --> T[Pruebas]
  T --> M[Memoria]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant Dev
  participant Spec
  participant Code
  participant CI
  Dev->>Spec: crea/actualiza spec
  Dev->>Code: implementa tareas
  Code->>CI: lint + tests + SAST
  CI-->>Dev: resultado
  Dev->>Spec: actualiza Memoria
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
