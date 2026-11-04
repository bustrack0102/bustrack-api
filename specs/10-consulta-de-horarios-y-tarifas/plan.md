# Plan técnico — 10-consulta-de-horarios-y-tarifas — Consulta de horarios y tarifas

> Spec: `spec.md` · Repositorio: BusTrack API · Estado: Pendiente.

## 1. Estrategia

Implementar por incrementos, conservando contratos y midiendo con pruebas antes de avanzar.

## 2. Arquitectura y componentes

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- Componentes: componentes del repositorio afectados por la tarea.
- Tecnología: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.

## 3. Archivos y estructura

```text
API_SDD/Controllers/V1/Horarios*, API_SDD/Controllers/V1/Tarifas*, specs-endpoints/21-endpoint-horarios/, specs-endpoints/22-endpoint-tarifas/
```

## 4. Seguridad

- ASVS L2, Argon2id, validación JWT estricta, rate limiting, CSP con nonce, Serilog scrubbing y cero secretos en Git.
- Validar entradas y no registrar secretos/PII.

## 5. Riesgos

- Confundir el estado actual con el objetivo.
- Dependencias con otros repositorios no documentadas.
- Ampliar el alcance sin spec aprobada.

## 6. Estrategia de pruebas

Pruebas unitarias y de contrato cuando exista código; revisión documental mientras la tarea sea propuesta.

## 7. Trazabilidad

Cada tarea de `task.md` referencia un requisito de `spec.md`; los diagramas viven en `chart.md`.
