---
name: cosmos-migration
description: Migrar de SQL Server a Cosmos DB sin romper contratos
metadata:
  repo: BusTrack API
---

# cosmos-migration

## Cuándo usar

Se planifica `specs/12-migracion-de-persistencia-a-cosmos-db/`.

## Pasos

1. Mapear agregados a documentos.
2. Definir particiones e índices.
3. Diseñar doble escritura y rollback.

## Prohibido

- Copiar patrones de otro stack sin adaptación.
- Declarar recursos o endpoints inexistentes.
- Introducir secretos o PII.
