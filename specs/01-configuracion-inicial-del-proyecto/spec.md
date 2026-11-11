# 01-configuracion-inicial-del-proyecto — Configuración inicial del proyecto

> Repositorio: BusTrack API · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Dejar la solución ASP.NET Core .NET 10 restaurable, configurable por entorno y ejecutable en local.

## 2. Contexto

La solución API_SDD.slnx existe con configuración, logging y arranque. El build estándar falla por NU1902/NU1903.

## 3. Alcance

**Incluye:**

- Solución y proyectos.
- appsettings por entorno.
- Logging y arranque.
- Comandos de build.

**No incluye:**

- Cambiar el stack objetivo.
- Desplegar recursos o ejecutar migraciones en esta fase.

## 4. Requisitos funcionales

1. Restaurar y compilar la solución.
2. Separar configuración dev/test/prod.
3. No incluir secretos reales.
4. Documentar el bloqueo de NuGetAudit.

## 5. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad tarea -> prueba -> evidencia.

## 6. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 7. Criterios de aceptación

- [ ] La tarea tiene criterios verificables y evidencia asociada.
- [ ] No se declara implementado lo que no existe en código o pruebas.
- [ ] Los contratos consumidos o expuestos quedan documentados.
- [ ] `Memoria.md` refleja el estado resultante.

## 8. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`

## 9. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
