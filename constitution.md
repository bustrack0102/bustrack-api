# Constitución — BusTrack API

> Fuente de verdad del repositorio. Versión 2.0.0 (2026-10-30). Todo cambio de arquitectura
> exige spec y registro en `Memoria.md`.

## 1. Propósito

BusTrack API forma parte del ecosistema BusTrack. API central tiene como responsabilidad:
Expone REST a Flutter y Angular; consume ML/GenAI por HTTP interno y es la fuente autorizada de datos.

## 2. Responsabilidades

- Mantener una responsabilidad delimitada dentro de BusTrack.
- Consumir o exponer contratos HTTP documentados.
- No duplicar datos ni lógica que pertenecen a otro servicio.
- Aplicar SDD: toda funcionalidad nace de un spec aprobado.

## 3. Arquitectura y tecnologías

- **Actual:** ASP.NET Core .NET 10, EF Core + SQL Server, JWT HS256, Argon2id, FluentValidation, Serilog, Scalar/OpenAPI, xUnit.
- **Objetivo:** ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- Cambiar la tecnología objetivo requiere autorización explícita y un spec de migración.

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.
- ML y GenAI se consumen como servicios HTTP internos autenticados.
- No hay rastreo GPS continuo ni datos inventados por IA.

## 4. Principios de diseño

- Separación de responsabilidades y dependencias dirigidas al dominio.
- Contratos explícitos, validados y versionados.
- Configuración por entorno y cero secretos en el repositorio.
- Real-first: no se declara implementado lo que no existe en código o pruebas.
- `Memoria.md` refleja el estado real, no el deseado.

## 5. Seguridad

ASVS L2, Argon2id, validación JWT estricta, rate limiting, CSP con nonce, Serilog scrubbing y cero secretos en Git.

- Prohibido registrar tokens, contraseñas, claves o PII.
- Los secretos viven en variables de entorno o Azure Key Vault.
- Toda entrada externa se valida y toda salida se codifica.

## 6. Calidad y pruebas

- xUnit, FluentAssertions, Moq, FsCheck, Testcontainers.MsSql y WebApplicationFactory.
- Sin pruebas y evidencia de ejecución no hay terminado.
- Los gates de CI para lint, pruebas y SAST son bloqueantes.
- Los tests triviales no cuentan como cobertura.

## 7. Convenciones

- Código e identificadores en inglés; documentación y mensajes de usuario en español.
- Formato y lint según el stack objetivo.
- PR pequeños y trazables a un spec.
- Prohibido `TODO` sin issue asociado.

## 8. Restricciones

- No rastreo GPS continuo de usuarios ni unidades.
- No acceso directo de clientes a Cosmos DB.
- No invención de rutas, horarios, tarifas, empresas o destinos por parte de IA.
- No recolección de sensores sin consentimiento explícito.
- No se declaran recursos Azure desplegados.

## 9. Integración con BusTrack

- Contratos transversales: `docs/contratos-integracion.md`.
- La API central es la fuente autorizada de datos de transporte.
- ML y GenAI son servicios internos.

## 10. Gobernanza

- Obligatorios: este documento, `AGENTS.md`, `Memoria.md`, `CHECKLIST_PR.md` y `specs/`.
- Las decisiones no resueltas se registran como pendientes; no se inventan respuestas.
