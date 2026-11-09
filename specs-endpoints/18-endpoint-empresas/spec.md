# 18-endpoint-empresas — Endpoint de empresas y asociaciones

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/empresas` · Estado: Implementado parcialmente (ver evidencia) · Fecha: 2026-11-09.

## 1. Objetivo

Listar empresas, asociaciones y medios de transporte. Persistencia objetivo: Cosmos DB for NoSQL; implementación provisional sobre MongoDB for Atlas (deuda técnica registrada en `Memoria.md`).

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta base | `/api/v1/empresas` |
| Autenticación | Lectura pública con `[AllowAnonymous]` en estos endpoints, hasta cerrar la decisión de la matriz auth (sección 4). Los endpoints de sesión (login/logout) sí usan flujo JWT HS256. |
| Consumidor | Flutter y Angular |
| Estado | Implementado parcialmente |

**Endpoints implementados (V1):**

| Ruta | Descripción | Estado |
|---|---|---|
| `GET /api/v1/empresas` | Listado paginado con filtros (`q`, `medioTransporteId`, `municipio`, `calificacionMin`, `activo`, `ordenarPor`) y ordenación (`nombre`/`rutas`/default calificación). | Implementado |
| `GET /api/v1/empresas/autocompletar` | Autocompletado por texto, máx. `limite` (default 10). | Implementado |
| `GET /api/v1/empresas/{id}` | Detalle y rutas asociadas. | Implementado |

**Parámetros / entrada:**

- Query: `pagina` (default 1), `tamanoPagina`, `q`, `medioTransporteId`, `municipio`, `calificacionMin`, `activo`, `ordenarPor`.

**Respuesta:**

- 200: lista paginada (`PagedResult<EmpresaListDto>`) / autocompletado / `EmpresaDetailDto`.
- 404: empresa inexistente o inactiva (detalle).
- 500: error interno sin detalles. **Nota:** si `MongoDb:ConnectionString` no está configurado, `MongoDbContext` lanza al resolver la dependencia (fail-fast), degradando a 500 — pendiente mapear a 503.

## 3. Códigos HTTP

- 200: éxito.
- 400/422: validación.
- 404: recurso inexistente (solo detalle).
- 500: error interno sin detalles.
- 429: rate limit (`Global`).

## 4. Validaciones

1. Paginación acotada (pendiente implementar límite estricto de `tamanoPagina` — deuda).
2. Filtros permitidos según el contrato documentado.

## 5. Seguridad

- ASVS L2, Argon2id, validación JWT estricta, rate limiting, CSP con nonce, Serilog scrubbing y cero secretos en Git.
- Cadena Mongo: solo vía `appsettings.Local.json` o User Secrets; nunca en el repo.
- No registrar tokens, credenciales ni PII.

## 6. Criterios de aceptación

- [x] La tarea tiene criterios verificables y evidencia asociada. Evidencia: compilación Release limpia; `dotnet test UnitTest -c Release` 94/94 (2026-11-09).
- [x] No se declara implementado lo que no existe en código. El código está en `main` (`b256ab0`); **deuda**: no hay pruebas de integración contra MongoDB real ni tests unitarios de `EmpresaService` — por eso el estado es "parcialmente".
- [x] Los contratos consumidos o expuestos quedan documentados (ver `docs/contratos-integracion.md`).
- [x] `Memoria.md` refleja el estado resultante.

## 7. Dependencias

- `docs/contratos-integracion.md`
- `specs-endpoints/27-endpoint-empresa-individual/` (detalle, slice relacionado)
- Registro DI: `API_SDD/Hosting/ServiceCollectionExtensions.cs`
