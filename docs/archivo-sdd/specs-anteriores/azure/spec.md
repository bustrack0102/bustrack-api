# Arquitectura Azure propuesta

> Repositorio: BusTrack API · Ruta: `specs/azure/` · Estado: Propuesta · Fecha: 2026-10-30.

## 1. Identificador y nombre

- **ID:** `azure`
- **Nombre:** Arquitectura Azure propuesta
- **Repositorio:** BusTrack API

## 2. Objetivo

Documentar la arquitectura Azure objetivo de API central sin declarar recursos desplegados.

## 3. Contexto

Servicio propuesto: Azure App Service Linux (.NET 10) o Container Apps + Cosmos DB for NoSQL + Key Vault + Application Insights. Ningún recurso está desplegado.

## 4. Alcance

**Incluye:**

- Servicio recomendado y responsabilidad.
- Configuración y variables.
- Secretos y conectividad.
- Monitoreo, escalabilidad y costos estimados.

**No incluye:**

- Provisionar recursos.
- Publicar credenciales.
- Afirmar despliegue existente.

## 5. Requisitos funcionales

1. Identificar cada recurso y su responsabilidad.
2. Documentar variables de entorno.
3. Definir gestión de secretos y conectividad.
4. Definir monitoreo y escalabilidad.

## 6. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad spec -> tarea -> prueba.

## 7. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 8. Criterios de aceptación

- [ ] El documento distingue propuesta de estado real.
- [ ] Variables y secretos listados.
- [ ] Monitoreo y escalabilidad definidos.
- [ ] No hay afirmaciones de despliegue.

## 9. Restricciones

- No ejecutar migraciones ni despliegues en esta fase.
- No cambiar el stack objetivo sin autorización.
- No incluir secretos ni PII.

## 10. Dependencias

- `docs/contratos-integracion.md`
- specs/seguridad/

## 11. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.
