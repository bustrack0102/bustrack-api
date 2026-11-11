# 31-endpoint-ruta-individual — Endpoint de detalle de ruta

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/rutas/{id}` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Consultar el detalle integral de una ruta de transporte específica, incluyendo su trazado geométrico oficial (coordenadas del recorrido), empresa operadora, origen, destino y resumen de paradas.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/rutas/{id}` |
| Autenticación | Opcional / Pública |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Path: `id` (GUID identificador único de la ruta).

**Respuesta:**

- 200: Objeto JSON con el detalle completo de la ruta y trazado.
- 400: GUID inválido.
- 404: Ruta no encontrada.

```json
{
  "id": "7ee293a8-9b5b-89a6-f73a-603a73a9e0e1",
  "codigo": "R-101",
  "nombre": "Terminal Sur - Zona 10",
  "empresaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "empresaNombre": "Transportes Metropolitanos S.A.",
  "medioTransporte": "Autobús Urbano",
  "origen": "Terminal Central Sur",
  "destino": "Plaza España Zona 10",
  "longitudKm": 18.5,
  "tiempoEstimadoMinutos": 45,
  "trazadoGeoJson": "{\"type\":\"LineString\",\"coordinates\":[[-90.5891,14.5312],[-90.5721,14.5510],[-90.5134,14.5982]]}",
  "activo": true
}
```

## 3. Códigos HTTP

- 200: Detalle obtenido exitosamente.
- 400: GUID con formato incorrecto.
- 404: Ruta inexistente.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. `id` debe tener formato GUID válido.
2. La ruta debe provenir de registros oficiales en Cosmos DB (sin invención ni trazados generados por IA).

## 5. Seguridad

- ASVS L2, rate limiting.
- Geometrías de trazado fijas validadas para evitar inyección de payloads de geodatos excesivos.

## 6. Criterios de aceptación

- [ ] Devuelve 200 con trazado geográfico oficial y metadatos de operación.
- [ ] Devuelve 404 si la ruta no existe.
- [ ] No utiliza rastreo GPS en tiempo real ni altera datos de transporte (Constitución §8).

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/09-consulta-de-rutas-y-paradas/`
- `specs-endpoints/19-endpoint-rutas/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
