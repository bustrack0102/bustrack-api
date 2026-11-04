# Diagramas — 03-configuracion-de-seguridad — Configuración de seguridad

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Credencial] --> B[Argon2id]
  B --> C[JWT]
  C --> D[Validación]
  D --> E[Acceso]
  E --> F[Log sin PII]
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

Semgrep quedó como deuda hasta que las reglas se validen en CI.
