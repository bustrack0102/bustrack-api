# Specs por tarea — BusTrack API

> Cada carpeta numerada representa una tarea o funcionalidad concreta y contiene
> `spec.md`, `plan.md`, `task.md` y `chart.md`. Las categorías técnicas generales
> ya no viven aquí: CI/CD está en `../cicd/` y Azure en `../azure/`.
> El historial se conserva en `../docs/archivo-sdd/specs-anteriores/`.

## Tareas

| ID | Tarea | Estado |
|---|---|---|
| `01-configuracion-inicial-del-proyecto` | Configuración inicial del proyecto | Implementado |
| `02-arquitectura-base-y-pipeline-http` | Arquitectura base y pipeline HTTP | Implementado |
| `03-configuracion-de-seguridad` | Configuración de seguridad | Parcial |
| `04-gestion-de-usuarios` | Gestión de usuarios | Implementado |
| `05-autenticacion-jwt` | Autenticación con JWT | Implementado |
| `06-gestion-de-sesiones-y-logout` | Gestión de sesiones y logout | Implementado |
| `07-modelo-de-dominio-de-transporte` | Modelo de dominio de transporte | Pendiente |
| `08-consulta-de-empresas-y-asociaciones` | Consulta de empresas y asociaciones | Pendiente |
| `09-consulta-de-rutas-y-paradas` | Consulta de rutas y paradas | Pendiente |
| `10-consulta-de-horarios-y-tarifas` | Consulta de horarios y tarifas | Pendiente |
| `11-gestion-de-calificaciones` | Gestión de calificaciones | Pendiente |
| `12-migracion-de-persistencia-a-cosmos-db` | Migración de persistencia a Cosmos DB for NoSQL | Pendiente |

## Reglas

- No crear carpetas vacías ni specs genéricas para completar la estructura.
- No dividir una tarea en varias specs ni agrupar funcionalidades independientes.
- Una tarea pasa a `Implementado` solo con código y pruebas verificables.
- Los endpoints se documentan aparte, en `../specs-endpoints/`.
