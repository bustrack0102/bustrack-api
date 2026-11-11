# 29-endpoint-destinos — Endpoint de destinos y terminales

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/destinos` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Consultar el catálogo oficial de destinos, terminales de transferencia y cabeceras municipales para permitir la búsqueda de rutas por origen y destino.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/destinos` |
| Autenticación | Opcional / Pública |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Query: `zona` (filtro por zona o municipio), `search` (texto de búsqueda), `page`, `pageSize`.

**Respuesta:**

- 200: Lista paginada `PagedResult<DestinoDto>`.
- 400: Parámetros inválidos.

```json
{
  "items": [
    {
      "id": "5cc07186-7939-6784-d51e-4e185188cfc8",
      "nombre": "Terminal Central Sur",
      "municipio": "Villa Nueva",
      "departamento": "Guatemala",
      "latitud": 14.5312,
      "longitud": -90.5891,
      "activo": true
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

## 3. Códigos HTTP

- 200: Consulta exitosa.
- 400: Parámetros de consulta incorrectos.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. Solo destinos registrados en el catálogo oficial (sin invención por IA ni extrapolación no autorizada).
2. Paginación máxima de 50 registros por petición.

## 5. Seguridad

- Restricción estricta de negocio: Prohibida la invención o generación de destinos por IA (Constitución §8).

## 6. Criterios de aceptación

- [ ] Los destinos provienen de la base de datos oficial.
- [ ] Soporta filtrado por municipio o nombre.
- [ ] Respuesta con coordenadas geográficas fijas del punto de llegada/terminal.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/07-modelo-de-dominio-de-transporte/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
