# Diagramas — 12-migracion-de-persistencia-a-cosmos-db — Conexión a Azure DocumentDB (BusTrackDB)

> Spec: `spec.md` · Plan: `plan.md` · Fecha: 2026-11-23.

## 1. Flujo de Migración de Persistencia

```mermaid
flowchart TD
  subgraph Origen
    A["MongoDB Atlas (BusTrackNoSQL)"]
  end
  subgraph Azure DMS
    B["Azure Database Migration Service<br/>(34 colecciones / 0 errores)"]
  end
  subgraph Destino
    C["Azure DocumentDB vCore<br/>Cluster: bustrack-db<br/>DB: BusTrackDB"]
  end
  A -->|Migración completada| B
  B -->|Datos sincronizados| C
```

## 2. Arquitectura de Conexión de la API

```mermaid
flowchart LR
  API["BusTrack API<br/>(ASP.NET Core .NET 10)"] -->|"MongoClient (MongoDB.Driver 3.5.1)<br/>SCRAM-SHA-256 + TLS"| DocDB["Azure DocumentDB vCore<br/>(bustrack-db.global.mongocluster.cosmos.azure.com)"]
  DocDB --> DB["Base de Datos: BusTrackDB<br/>(34 colecciones activas)"]
  API -->|"Health Check: ping"| DocDB
```

## 3. Secuencia de Inicialización y Sanitización

```mermaid
sequenceDiagram
  participant App as BusTrack API Host
  participant Ctx as MongoDbContext
  participant Sanitizer as ConfigurationSanitizer
  participant Driver as MongoClientSettings
  participant DocDB as Azure DocumentDB

  App->>Ctx: Inyecta IConfiguration
  Ctx->>Sanitizer: Valida y limpia ConnectionString
  alt Es placeholder o vacío
    Ctx-->>App: IsConfigured = false (Modo degradado / mock)
  else Cadena válida con credenciales
    Ctx->>Driver: FromConnectionString(TLS, SCRAM-SHA-256)
    Ctx->>DocDB: Conexión y Pool Singleton a BusTrackDB
    DocDB-->>Ctx: Conexión establecida
    Ctx-->>App: IsConfigured = true
  end
```
