# Memoria técnica — BusTrack API

> Registro de estado, decisiones arquitectónicas con justificación, lecciones aprendidas y deuda técnica del proyecto BusTrack.

---

## 📌 Fase 00 — Inicialización y Estructura SDD (2026-10-29)

- **Estado:** Estructura base Spec-Driven Development (SDD) creada a partir de las especificaciones y lineamientos oficiales.
- **Decisión 1 (Metodología):** Adopción estricta de SDD y DevSecOps. Toda funcionalidad nace de una especificación en `specs/` antes de codificarse.
- **Decisión 2 (Persistencia):** Definición de MongoDB (NoSQL) como base de datos del proyecto para cumplir con los lineamientos de la asignatura.
- **Decisión 3 (Estructura de Pruebas):** División modular en `UnitTest/`, `IntegrationTest/`, `SecurityTest/`, `ContractTest/` y `DatabaseTest/` para validar cada capa de forma independiente.
- **Deuda Técnica Documentada:** Configuración de la conexión e infraestructura en la nube (Azure) pendiente de parametrización en etapas posteriores.
