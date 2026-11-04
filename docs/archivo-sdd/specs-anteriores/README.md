# Specs — BusTrack API

> Catálogo canónico. Cada spec contiene `spec.md`, `plan.md`, `task.md` y `chart.md`.
> Las specs históricas se conservan como referencia y **no** rigen el desarrollo.

## Specs canónicas

| ID | Nombre | Estado |
|---|---|---|
| `00-fundamentos` | 00 — Fundamentos y gobernanza SDD | Implementado |
| `01-configuracion` | 01 — Configuración del entorno | Parcial |
| `02-arquitectura` | 02 — Arquitectura y límites del servicio | Parcial |
| `endpoints/login` | Endpoint / flujo de login | Implementado |
| `endpoints/logout` | Cierre de sesión / revocación | Implementado |
| `integraciones` | Integraciones con el ecosistema BusTrack | Parcial |
| `seguridad` | Seguridad y privacidad | Parcial |
| `pruebas` | Estrategia de pruebas | Parcial |
| `cicd` | CI/CD y gates de calidad | Parcial |
| `azure` | Arquitectura Azure propuesta | Propuesta |
| `migraciones/cosmos-db` | Migración SQL Server/EF Core → Azure Cosmos DB for NoSQL | Pendiente |
| `dominio/transporte` | Dominio de transporte (empresas, rutas, paradas, destinos, horarios, tarifas, calificaciones) | Pendiente |

## Historial

- `nv-00` … `nv-06` y `_template.md` (histórico del backend .NET).

## Reglas

- No copiar specs de otro stack.
- Toda spec nueva debe declarar objetivo, alcance, requisitos, criterios de aceptación y dependencias.
- Una spec solo pasa a `Implementado` con código y pruebas verificables.
- `Memoria.md` es la fuente del estado real.
