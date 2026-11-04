# Contratos de integración — BusTrack API

> Vista de los contratos que este repositorio expone o consume. Solo login/logout están
> implementados en los clientes y la API; el resto es **propuesta** hasta que exista código.

## 1. Principios

- La API central es la única fuente autorizada de datos de transporte.
- Ningún cliente accede a Cosmos DB ni a ML/GenAI directamente.
- ML y GenAI son servicios internos autenticados; no exponen login público.
- Toda integración usa HTTPS, timeouts y errores tipados.
- Lo propuesto se marca como tal; no se declaran endpoints inexistentes.

## 2. Expone (clientes)

| Método | Ruta | Estado | Consumidor | Notas |
|---|---|---|---|---|
| POST | `/api/v1/auth/login` | Implementado | Flutter, Angular | JWT HS256; lockout. |
| POST | `/api/v1/auth/logout` | Implementado | Flutter, Angular | Blacklist en IMemoryCache. |
| GET/POST/PUT/DELETE | `/api/v1/usuarios` | Implementado | Admin | CRUD con paginación y RowVersion. |
| GET | `/api/v1/transporte/*` | Propuesta | Flutter, Angular | Empresas, rutas, paradas, destinos, horarios, tarifas. |
| POST | `/api/v1/calificaciones` | Propuesta | Flutter | Calificaciones verificadas. |

## 3. Consume (servicios internos)

| Servicio | Operación | Estado | Auth |
|---|---|---|---|
| ML | `POST /inference` (lote de acelerómetro) | Propuesta | Token interno / identidad administrada. |
| GenAI | `POST /recommendations` (contexto validado) | Propuesta | Token interno / identidad administrada. |

## 4. Persistencia

- **Actual:** SQL Server vía EF Core.
- **Objetivo:** Azure Cosmos DB for NoSQL; solo la API la usa.

## 5. Errores uniformes

| Código | Significado |
|---|---|
| 400 | Petición mal formada. |
| 401 | Credencial ausente o inválida. |
| 403 | Sin permiso. |
| 404 | Recurso inexistente. |
| 409 | Conflicto de concurrencia/unicidad. |
| 422 | Validación de dominio/esquema. |
| 429 | Rate limit. |
| 500 | Error interno sin detalles sensibles. |
