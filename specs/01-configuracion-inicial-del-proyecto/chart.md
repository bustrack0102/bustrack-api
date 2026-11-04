# Diagramas — 01-configuracion-inicial-del-proyecto — Configuración inicial del proyecto

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo funcional

```mermaid
flowchart TD
  A[Clonar] --> B[Restore]
  B --> C[Configurar entorno]
  C --> D[Build]
  D --> E[Test]
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

El build estándar está bloqueado por NuGetAudit; no se desactiva sin autorización.
