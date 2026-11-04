# Seguridad y privacidad

> Repositorio: BusTrack API · Ruta: `specs/seguridad/` · Estado: Parcial · Fecha: 2026-10-30.

## 1. Identificador y nombre

- **ID:** `seguridad`
- **Nombre:** Seguridad y privacidad
- **Repositorio:** BusTrack API

## 2. Objetivo

Fijar los controles de seguridad y privacidad de API central.

## 3. Contexto

ASVS L2, Argon2id, validación JWT estricta, rate limiting, CSP con nonce, Serilog scrubbing y cero secretos en Git.

## 4. Alcance

**Incluye:**

- Autenticación/autorización.
- Validación y codificación.
- Gestión de secretos.
- Logs y datos sensibles.
- Consentimiento de sensores cuando aplique.

**No incluye:**

- Implementar controles de otro servicio.
- Desplegar WAF o servicios Azure.

## 5. Requisitos funcionales

1. Validar todas las entradas.
2. No registrar secretos/PII.
3. Aplicar mínimo privilegio.
4. Documentar amenazas y controles.

## 6. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad spec -> tarea -> prueba.

## 7. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 8. Criterios de aceptación

- [ ] Controles documentados y probados.
- [ ] Semgrep ejecutable y bloqueante.
- [ ] Pruebas de seguridad en verde.
- [ ] Sin hallazgos críticos abiertos.

## 9. Restricciones

- No ejecutar migraciones ni despliegues en esta fase.
- No cambiar el stack objetivo sin autorización.
- No incluir secretos ni PII.

## 10. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`

## 11. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.
