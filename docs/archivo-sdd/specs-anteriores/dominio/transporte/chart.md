# Diagramas — Dominio de transporte (empresas, rutas, paradas, destinos, horarios, tarifas, calificaciones)

> Spec: `spec.md` · Plan: `plan.md`. Diagramas Mermaid; no representan recursos desplegados.

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

## 2. Relación entre componentes

```mermaid
flowchart TD
  API[API] --> EMP[Empresas]
  API --> RUT[Rutas]
  API --> PAR[Paradas]
  API --> DES[Destinos]
  API --> HOR[Horarios]
  API --> TAR[Tarifas]
  API --> CAL[Calificaciones]
```

## 3. Flujo de datos / secuencia

```mermaid
sequenceDiagram
  participant C as Cliente
  participant A as API
  participant D as Cosmos
  C->>A: GET /rutas?destino=X
  A->>D: consulta
  D-->>A: documentos
  A-->>C: rutas + paradas + horarios/tarifas
```

## 4. Notas

Ninguna entidad de transporte existe todavía en el código.
