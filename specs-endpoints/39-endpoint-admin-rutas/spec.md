# 39-endpoint-admin-rutas — Endpoint de administración de rutas y paradas

> Repositorio: BusTrack API · Endpoint: `POST/PUT/DELETE /api/v1/rutas` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Permitir a los administradores registrar nuevas rutas oficiales, actualizar trazados geográficos, asignar secuencias de paradas y deshabilitar recorridos en el sistema.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST /api/v1/rutas`, `PUT /api/v1/rutas/{id}`, `DELETE /api/v1/rutas/{id}` |
| Ruta | `/api/v1/rutas` y `/api/v1/rutas/{id}` |
| Autenticación | JWT con claim `role: Admin` (`AdminPolicy`) |
| Consumidor | Angular (Panel de Administración) |
| Estado | Propuesto |

**Operaciones:**

- `POST /api/v1/rutas`: Da de alta una nueva ruta con su trazado y empresa asignada. Retorna 201 Created.
- `PUT /api/v1/rutas/{id}`: Modifica trazado, paradas intermedias o metadatos de ruta. Control de concurrencia optimista. Retorna 200 OK.
- `DELETE /api/v1/rutas/{id}`: Desactiva la ruta del catálogo público (`activo = false`). Retorna 204 No Content.

**Body POST:**

```json
{
  "codigo": "R-101",
  "nombre": "Terminal Sur - Zona 10",
  "empresaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "medioTransporteId": 1,
  "origen": "Terminal Central Sur",
  "destino": "Plaza España Zona 10",
  "longitudKm": 18.5,
  "tiempoEstimadoMinutos": 45,
  "trazadoGeoJson": "{\"type\":\"LineString\",\"coordinates\":[[-90.5891,14.5312],[-90.5134,14.5982]]}",
  "paradasIdsOrdenadas": [
    "8ff3a4b9-0c6c-9ab7-e84b-714b84b0f1f2",
    "9aa4b5c0-1d7d-0bc8-f95c-825c95c1a2a3"
  ]
}
```

## 3. Códigos HTTP

- 201: Ruta creada.
- 200: Ruta actualizada.
- 204: Ruta desactivada.
- 400: Parámetros o formato GeoJSON inválido.
- 401: No autenticado.
- 403: Requiere rol de Administrador.
- 404: Ruta, empresa o parada no encontrada.
- 409: Código de ruta duplicado o conflicto de versión.
- 422: Fallo de validación de negocio.
- 429: Rate limit de escrituras.
- 500: Error interno no controlado.

## 4. Validaciones

1. Código de ruta único en el sistema.
2. `empresaId` debe corresponder a una empresa activa existente.
3. Las paradas en la lista ordenada deben existir y estar activas.
4. GeoJSON de trazado validado sintácticamente.

## 5. Seguridad

- ASVS L2, AdminPolicy estricta.
- Sanitización de entradas y auditoría de cambios en Serilog.

## 6. Criterios de aceptación

- [ ] Administrador crea ruta asociando paradas y trazado.
- [ ] Valida unicidad del código de ruta.
- [ ] Rechaza sin privilegios de administrador.

## 7. Dependencias

- `constitution.md`
- `specs/07-modelo-de-dominio-de-transporte/`
- `specs/09-consulta-de-rutas-y-paradas/`
- `specs-endpoints/19-endpoint-rutas/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
