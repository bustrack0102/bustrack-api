# CI/CD y gates de calidad

> Repositorio: BusTrack API · Ruta: `specs/cicd/` · Estado: Parcial · Fecha: 2026-10-30.

## 1. Identificador y nombre

- **ID:** `cicd`
- **Nombre:** CI/CD y gates de calidad
- **Repositorio:** BusTrack API

## 2. Objetivo

Definir el pipeline de BusTrack API con gates bloqueantes y artefactos verificables.

## 3. Contexto

Herramientas objetivo: GitHub Actions, Semgrep, NuGetAudit, Stryker advisory y cobertura mínima 45%.

## 4. Alcance

**Incluye:**

- Lint, formato, pruebas, SAST, SCA y secretos.
- Artefactos y cobertura.
- Reglas de rama y PR.
- Sin despliegue real en esta fase.

**No incluye:**

- Despliegue automático a Azure.
- Servicios de pago.

## 5. Requisitos funcionales

1. Todo PR ejecuta lint + pruebas + SAST.
2. Los fallos bloquean la fusión.
3. No se usan secretos productivos en PR.
4. Se conservan artefactos útiles.

## 6. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad spec -> tarea -> prueba.

## 7. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 8. Criterios de aceptación

- [ ] El workflow se ejecuta en PR.
- [ ] Un fallo de lint/test/SAST pone el check en rojo.
- [ ] La cobertura se genera o se documenta su ausencia.

## 9. Restricciones

- No ejecutar migraciones ni despliegues en esta fase.
- No cambiar el stack objetivo sin autorización.
- No incluir secretos ni PII.

## 10. Dependencias

- .github/workflows/
- specs/pruebas/
- specs/seguridad/

## 11. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.
