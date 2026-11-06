# 28-endpoint-asociaciones — Endpoint de cooperativas y asociaciones de transporte

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/asociaciones` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Listar y consultar información de cooperativas, gremios y asociaciones de transporte que agrupan a múltiples empresas permisionarias.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/asociaciones` |
| Autenticación | Opcional / Pública |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Query: `page` (número de página, default 1), `pageSize` (tamaño, max 50), `search` (término de búsqueda textual opcional).

**Respuesta:**

- 200: Lista paginada `PagedResult<AsociacionDto>`.
- 400: Parámetros de paginación inválidos.

```json
{
  "items": [
    {
      "id": "4bb96075-6828-5673-c40d-3d074077bfb7",
      "nombre": "Asociación de Concesionarios del Norte",
      "siglas": "ASOCONOR",
      "zonaCobertura": "Zona Norte Metropolitana",
      "activo": true,
      "cantidadEmpresas": 8
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

## 3. Códigos HTTP

- 200: Lista devuelta exitosamente.
- 400: Parámetros de consulta fuera de rango.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. `page` mayor o igual a 1.
2. `pageSize` entre 1 y 50.
3. Sanitización de caracteres en `search`.

## 5. Seguridad

- ASVS L2, rate limit global, paginación acotada para evitar ataques de denegación de servicio por memoria.

## 6. Criterios de aceptación

- [ ] Devuelve 200 con la lista paginada y metadatos de paginación.
- [ ] Soporta filtro por término de búsqueda.
- [ ] Devuelve 400 cuando `page` o `pageSize` son inválidos.

## 7. Dependencias

- `docs/contratos-integracion.md`
- `specs/08-consulta-de-empresas-y-asociaciones/`

