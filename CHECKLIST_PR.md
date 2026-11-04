# CHECKLIST_PR.md — BusTrack API

> Completar en cada Pull Request. Una casilla sin evidencia no cuenta.

## Spec y alcance

- [ ] Existen `spec.md`, `plan.md`, `task.md` y `chart.md` para el cambio.
- [ ] El PR implementa solo el alcance aprobado.
- [ ] No se declara implementado lo que no existe en código/pruebas.
- [ ] `Memoria.md` actualizado con estado y decisión.

## Arquitectura

- [ ] Se respeta la tecnología objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- [ ] No se introducen dependencias o servicios no aprobados.
- [ ] Se respetan los contratos de `docs/contratos-integracion.md`.
- [ ] No hay acceso de clientes a Cosmos DB ni duplicación de la API.

## Calidad

- [ ] Formateo y lint en verde.
- [ ] Build en verde o bloqueo real documentado.
- [ ] Sin código muerto, placeholders ni `TODO` sin issue.
- [ ] Sin secretos, tokens ni PII en código, logs o fixtures.

## Seguridad

- [ ] Entradas validadas y salidas codificadas.
- [ ] Autenticación/autorización revisadas cuando aplica.
- [ ] Semgrep sin hallazgos nuevos.
- [ ] Dependencias escaneadas y vulnerabilidades justificadas.

## Pruebas

- [ ] Pruebas unitarias nuevas/actualizadas.
- [ ] Pruebas de integración/contrato si cambia un endpoint.
- [ ] Pruebas de seguridad si cambia auth, sesión o datos sensibles.
- [ ] Evidencia de ejecución adjunta (comando y resultado).

## Documentación y compatibilidad

- [ ] README/endpoints/contratos actualizados.
- [ ] Enlaces relativos válidos.
- [ ] Cambios compatibles o versionados.
- [ ] Migración documentada si cambia stack o persistencia.

## Integración

- [ ] El contrato consumido por Flutter/Angular/API sigue vigente.
- [ ] No se rompe a otro repositorio sin spec de migración.
- [ ] Variables de entorno documentadas; sin valores reales.

## Revisión

- [ ] Un revisor adicional aprobó.
- [ ] CI en verde.
- [ ] No se hizo commit/push/despliegue sin autorización.
