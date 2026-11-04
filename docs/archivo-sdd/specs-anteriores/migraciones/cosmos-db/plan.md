# Plan técnico — Migración SQL Server/EF Core → Azure Cosmos DB for NoSQL

> Spec: `spec.md` · Repositorio: BusTrack API · Estado: Pendiente.

## 1. Estrategia técnica

Documentar el estado actual y el objetivo; implementar por incrementos con pruebas y sin romper contratos.

## 2. Arquitectura propuesta

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- Componentes: Componentes del repositorio afectados por la spec.
- Tecnologías: Microsoft.Azure.Cosmos + ASP.NET Core .NET 10.

## 3. Componentes afectados

- Componentes del repositorio afectados por la spec

## 4. Estructura de archivos

```text
API_SDD/Data (nuevo), scripts de migración, specs/migraciones/cosmos-db/
```

## 5. Dependencias

- `docs/contratos-integracion.md`
- specs/02-arquitectura/

## 6. Consideraciones de seguridad

- ASVS L2, Argon2id, validación JWT estricta, rate limiting, CSP con nonce, Serilog scrubbing y cero secretos en Git.

- Validar entradas y no registrar secretos/PII.

## 7. Riesgos

- Confundir el estado actual con el objetivo.
- Documentación genérica o desalineada.
- Dependencias entre repos no documentadas.

## 8. Estrategia de pruebas

Revisión de coherencia documental y, cuando exista código, pruebas unitarias, de contrato y seguridad.

## 9. Trazabilidad

Cada tarea de `task.md` referencia un requisito de `spec.md`. Los diagramas están en `chart.md`.
