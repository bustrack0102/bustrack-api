# Diagramas — Seguridad y privacidad

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Entrada] --> B[Validar]
  B --> C[Autenticar/Autorizar]
  C --> D[Procesar]
  D --> E[Sanitizar salida/log]
  E --> F[Responder]
```

## 2. Relación entre componentes

```mermaid
flowchart LR
  S[SAST] --> CI[CI]
  D[SCA] --> CI
  T[Pruebas seg.] --> CI
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as App
  participant L as Logs
  C->>A: request
  A->>A: validar + autorizar
  A->>L: evento sin PII
  A-->>C: respuesta
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.
