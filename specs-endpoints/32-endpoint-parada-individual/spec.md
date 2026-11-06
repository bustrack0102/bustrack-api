# 32-endpoint-parada-individual — Endpoint de detalle y cercanía de paradas

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/paradas/{id}` y `GET /api/v1/paradas/cercanas` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Consultar la información detallada de una parada de transporte específica y buscar paradas registradas en un radio geográfico determinado a partir de un punto de consulta.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/paradas/{id}` y `/api/v1/paradas/cercanas` |
| Autenticación | Opcional / Pública |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Ruta individual: `id` (GUID de la parada en path).
- Paradas cercanas: Query params `lat` (latitud decimal), `lng` (longitud decimal), `radioMetros` (entero, default 500, max 3000).

**Respuesta:**

- 200: Detalle de parada o lista de paradas cercanas ordenadas por distancia calculada.
- 400: Coordenadas o radio fuera de rango.
- 404: Parada no encontrada.

```json
{
  "id": "8ff3a4b9-0c6c-9ab7-e84b-714b84b0f1f2",
  "nombre": "Parada Central Trébol",
  "codigo": "P-TR-01",
  "latitud": 14.5982,
  "longitud": -90.5254,
  "distanciaMetros": 120.5,
  "rutasQueTransitan": [
    {
      "rutaId": "7ee293a8-9b5b-89a6-f73a-603a73a9e0e1",
      "codigoRuta": "R-101",
      "nombreRuta": "Terminal Sur - Zona 10"
    }
  ]
}
```

## 3. Códigos HTTP

- 200: Operación exitosa.
- 400: Coordenadas inválidas (lat fuera de [-90, 90], lng fuera de [-180, 180], radio > 3000).
- 404: Parada no encontrada en `/paradas/{id}`.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. Solo paradas pre-registradas en el inventario oficial.
2. Geocálculo de distancia esférica (fórmula Haversine o funciones geoespaciales de Cosmos DB ST_DISTANCE).
3. No realiza rastreo GPS continuo (Constitución §8).

## 5. Seguridad

- ASVS L2, rate limiting, validación de rangos geográficos.
- Sin recolección no consentida de ubicación del usuario.

## 6. Criterios de aceptación

- [ ] Devuelve 200 con la parada individual y lista de rutas que transitan por ella.
- [ ] Búsqueda cercana devuelve paradas dentro del radio solicitado ordenadas de menor a mayor distancia.
- [ ] Devuelve 400 si latitud o longitud son inválidas.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/09-consulta-de-rutas-y-paradas/`
- `specs-endpoints/20-endpoint-paradas/`

