---
version: 1.0.0
ratified: 2026-10-29
last_amended: 2026-10-29
status: active
source_of_truth: .specify/memory/constitution.md
mirror: ./constitution.md
methodology: SDD
project: BusTrack - API
team:
  - Fátima Ximena García González
  - Diego Parra Cruz
  - José De Jesús López Islas
  - Melanie Joliee Bonilla Domínguez
institution: Universidad Tecnológica de Tula-Tepeji
---

# Constitución — BusTrack API

> Rector SDD. Ante conflicto con otro artefacto, **manda esta constitución**.
> Filosofía madre: cada CRUD y funcionalidad sale liberable con todas las pruebas y gates aplicados.
> Persistencia oficial: MongoDB (NoSQL) conforme a los lineamientos de la materia.

## 1. Propósito y alcance

- API REST stateless para el sistema BusTrack (gestión de rutas, empresas, paradas, horarios, tarifas y calificaciones).
- Metodología SDD (Spec-Driven Development). Toda funcionalidad nace de una especificación en `/specs`.
- Objetivo: cada módulo o endpoint sigue el flujo `spec → modelo/DTO → servicio → controller → tests → gates → tag` liberable.

## 2. Principios no negociables

1. **Stateless**: sin sesiones en memoria del servidor. Autenticación mediante tokens JWT.
2. **Seguridad primero**: validación en frontera, denegación por defecto, mínimo privilegio, sin secretos en código ni logs.
3. **Fail-fast**: respuestas de error uniformes y códigos HTTP canónicos (`200, 201, 400, 401, 403, 404, 409, 422, 500`).
4. **Persistencia NoSQL**: Base de datos MongoDB como fuente de datos operativa.
5. **Observabilidad y Auditoría**: logging estructurado en formato JSON con CorrelationId para trazabilidad de peticiones.
6. **Documentación viva**: contratos reflejados en `docs/endpoints.md`, `rate-limit-matrix.md` y `asvs-l2-checklist.md`.
7. **Registro continuo**: Toda decisión arquitectónica o deuda técnica se documenta en `Memoria.md`.

## 3. Stack y componentes

| Capa | Definición |
|---|---|
| Runtime | API REST (.NET / C# o framework definido por el equipo) |
| Persistencia | MongoDB (NoSQL) |
| Caché | Redis (para consultas frecuentes) |
| Pruebas | Unit, Integration, Security, Contract, Database |
| Auth | JWT HS256 / Bearer token + expiración + logout |
| Validación | Validación estricta de esquemas y DTOs de entrada |
| SAST & Calidad | Semgrep, análisis de dependencias y scripts de validación |
| CI | GitHub Actions (`ci-pr.yml`) |

## 4. Estructura de pruebas (QA Gates)

- `UnitTest/`: Lógica interna, formateadores, reglas de dominio.
- `IntegrationTest/`: Flujos completos de endpoints y respuesta HTTP.
- `SecurityTest/`: Autenticación, protección de endpoints y headers HTTP.
- `ContractTest/`: Validación de esquemas y contratos de interfaz.
- `DatabaseTest/`: Operaciones y consultas sobre la base de datos NoSQL.

## 5. Control de cambios y Releases

- Cada cambio o Pull Request debe validar el `CHECKLIST_PR.md`.
- Versionamiento semántico y matriz de despliegue previa a cada liberación.
