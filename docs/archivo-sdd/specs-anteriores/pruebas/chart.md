# Diagramas — Estrategia de pruebas

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Código] --> B[Unit]
  B --> C[Integration]
  C --> D[Contract/Model/Guardrail]
  D --> E[Security]
  E --> F[Reporte]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  T[Tests] --> CI[CI]
  CI --> R[Reporte cobertura]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant Dev
  participant CI
  participant R as Reporte
  Dev->>CI: push
  CI->>CI: lint + tests + SAST
  CI->>R: cobertura
  R-->>Dev: evidencia
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
