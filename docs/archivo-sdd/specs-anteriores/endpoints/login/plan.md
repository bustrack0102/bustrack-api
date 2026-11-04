# Plan técnico — Endpoint / flujo de login

> Spec: `spec.md` · Repositorio: BusTrack API · Estado: Implementado.

## 1. Estrategia técnica

Documentar el estado actual y el objetivo; implementar por incrementos con pruebas y sin romper contratos.

## 2. Arquitectura propuesta

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- Componentes: Componentes del repositorio afectados por la spec.
- Tecnologías: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.

## 3. Componentes afectados

- Componentes del repositorio afectados por la spec

## 4. Estructura de archivos

```text
specs/endpoints/login/ + código y pruebas relacionadas
```

## 5. Dependencias

- API central
- `docs/contratos-integracion.md`

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
