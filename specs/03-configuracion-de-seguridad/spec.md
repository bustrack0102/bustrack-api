# 03-configuracion-de-seguridad — Configuración de seguridad

> Repositorio: BusTrack API · Estado: Parcial · Fecha: 2026-10-30.

## 1. Objetivo

Aplicar controles base: Argon2id, validación JWT estricta, gestión de secretos, scrubbing de logs y cabeceras.

## 2. Contexto

Argon2id, JWT HS256 y Serilog con scrubbing existen. Faltan reglas Semgrep efectivas y revisión ASVS completa.

## 3. Alcance

**Incluye:**

- Hash Argon2id.
- Validación JWT.
- Secretos por entorno.
- Scrubbing de logs.
- Checklist ASVS L2.

**No incluye:**

- Cambiar el stack objetivo.
- Desplegar recursos o ejecutar migraciones en esta fase.

## 4. Requisitos funcionales

1. Nunca almacenar contraseñas en claro.
2. Rechazar tokens inválidos o expirados.
3. No registrar secretos ni PII.
4. Documentar controles y brechas.

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
