# 30-endpoint-medios-transporte — Endpoint de medios y modalidades de transporte

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/medios-transporte` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Consultar el catálogo de tipos y medios de transporte público soportados por el sistema (ej. Autobús Urbano, Extraurbano/Interdepartamental, Microbús, Transmetro/BRT).

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/medios-transporte` |
| Autenticación | Opcional / Pública |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Query: `soloActivos` (booleano, default true).

**Respuesta:**

- 200: Lista de medios de transporte disponibles.

```json
[
  {
    "id": 1,
    "codigo": "BUS_URBANO",
    "nombre": "Autobús Urbano",
    "descripcion": "Servicio de transporte colectivo en el perímetro urbano",
    "icono": "directions_bus",
    "activo": true
  },
  {
    "id": 2,
    "codigo": "BRT",
    "nombre": "Sistema de Tránsito Rápido (BRT)",
    "descripcion": "Autobuses de tránsito rápido en carriles exclusivos",
    "icono": "directions_transit",
    "activo": true
  }
]
```

## 3. Códigos HTTP

- 200: Operación exitosa.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. Catálogo inmutable o administrado centralmente.
2. Caché HTTP recomendada mediante cabeceras `Cache-Control: public, max-age=3600`.

## 5. Seguridad

- ASVS L2, rate limiting global, sin datos sensibles.

## 6. Criterios de aceptación

- [ ] Devuelve la lista completa de modalidades activas.
- [ ] Incluye código mnemónico e identificador de icono para la interfaz cliente.

## 7. Dependencias

- `docs/contratos-integracion.md`
- `specs/07-modelo-de-dominio-de-transporte/`

