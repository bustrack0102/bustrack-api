# Diagramas — CI/CD y gates de calidad

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[PR] --> B[Lint]
  B --> C[Tests]
  C --> D[SAST/SCA]
  D --> E[Secretos]
  E --> F[Artefactos]
  F --> G[PR listo]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  PR[Pull Request] --> CI[GitHub Actions]
  CI --> C[Checks]
  C --> M[Merge]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant D as Dev
  participant G as GitHub
  participant CI as Actions
  D->>G: PR
  G->>CI: workflow
  CI-->>G: checks
  G-->>D: resultado
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
