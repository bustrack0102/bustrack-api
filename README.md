# BusTrack · API — Spec-Driven REST API

> Sistema de Consulta de Rutas y Transporte — **Universidad Tecnológica de Tula-Tepeji**
> Desarrollado con **Spec-Driven Development (SDD)** y asegurado bajo prácticas de **DevSecOps** desde el día uno.

---

## 👥 Equipo de Desarrollo (BusTrack)
* **Fátima Ximena García González**
* **Diego Parra Cruz**
* **José De Jesús López Islas**
* **Melanie Joliee Bonilla Domínguez**

---

## 📋 Resumen del Repositorio
Este repositorio contiene la **API REST** central del ecosistema BusTrack. Procesa y centraliza la información de rutas, paradas, horarios, tarifas, empresas, calificaciones y autenticación, sirviendo tanto a la aplicación móvil como a la plataforma web.

* **Metodología:** Spec-Driven Development (SDD) basada en especificaciones en `/specs`.
* **Seguridad y Calidad:** DevSecOps (SAST con Semgrep, análisis de dependencias, pruebas unitarias, de integración, contrato y seguridad, CI con GitHub Actions).
* **Base de Datos:** MongoDB (NoSQL) conforme a los lineamientos oficiales del proyecto.

---

## 🏛️ Componentes de Gobernanza y SDD

| Componente | Ruta | Propósito |
|---|---|---|
| **Constitución (Fuente de verdad)** | `.specify/memory/constitution.md` (espejo en `./constitution.md`) | Principios no negociables del proyecto y arquitectura de la API |
| **Manual operativo para Agentes** | `AGENTS.md` | Instrucciones, reglas de desarrollo y contratos de interfaces |
| **Especificaciones (Specs)** | `specs/` | Historias y requerimientos desglosados en especificaciones ejecutables (`spec.md`, `plan.md`, `tasks.md`) |
| **Plantillas SDD** | `.specify/templates/` (y `specs/_template.md`) | Plantillas oficiales para nuevas especificaciones y planes |
| **Agentes y Habilidades IA** | `.opencode/agents/` y `.opencode/skills/` | Configuración de agentes OpenCode / WARP |
| **Políticas de Seguridad SAST** | `.semgrep/semgrep.yaml` | Reglas de análisis estático de código |
| **Gates de Calidad y Scripts** | `scripts/` | Verificación de SDD, cobertura y reglas de compilación |
| **Checklist de PR** | `CHECKLIST_PR.md` | Lista de comprobación obligatoria antes de integrar cambios |
| **Memoria técnica** | `Memoria.md` | Registro de decisiones arquitectónicas, estado y deuda técnica |

---

## 🧪 Estructura de Pruebas

* `UnitTest/`: Pruebas unitarias aisladas de lógica de negocio y validaciones.
* `IntegrationTest/`: Pruebas de integración de endpoints y flujos completos.
* `SecurityTest/`: Pruebas de seguridad (autenticación JWT, headers seguros, sanitización).
* `ContractTest/`: Validación de contratos de entrada y salida (OpenAPI / Swagger).
* `DatabaseTest/`: Pruebas de persistencia y consultas con MongoDB.

---

## 🚀 Integración Continua (CI)
El flujo de integración continua está definido en `.github/workflows/ci-pr.yml` y valida compilación, análisis estático (SAST), pruebas y cumplimiento de gates de calidad en cada Pull Request.
