# Diagramas — 01 — Configuración del entorno

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Clonar] --> B[Configurar entorno]
  B --> C[Instalar dependencias]
  C --> D[Lint/tests]
  D --> E[Secretos de despliegue]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  E[Entorno local] --> C[CI]
  C --> KV[Key Vault]
  KV --> P[Producción]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant Dev
  participant CI
  participant KV as Key Vault
  Dev->>CI: push
  CI->>KV: secretos (solo despliegue)
  CI-->>Dev: resultado
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
