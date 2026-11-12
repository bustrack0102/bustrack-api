# 48-endpoint-mapa-elementos — Endpoint de elementos del mapa (EP-RP-10)

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/mapa/elementos` · Código: EP-RP-10 · Estado: Implementado · Fecha: 2026-11-11.

## 1. Objetivo

Exponer paradas y destinos georreferenciados filtrables por radio geográfico y tipo para renderización en `flutter_map` (OSM).

## 2. Contrato

- **Método:** `GET` · **Ruta:** `/api/v1/mapa/elementos` · **Auth:** Pública · **Rate:** Global.

## 3. Seguridad y Criterios

- Sin rastreo GPS continuo del usuario (Constitución §8).
- Respuestas cacheadas con Redis / `Cache-Control: public, max-age=120`.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
