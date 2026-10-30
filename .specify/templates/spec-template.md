# Spec Template — NV-0X (7 secciones + Aprobación)

> Constitución: `.specify/memory/constitution.md` §11. Sin cambio de contrato sin spec.

## 1. Objetivo
<!-- Qué CRUD/slice cubre. Filosofía 00+01+02+03+04 liberable. -->

## 2. Alcance / No-alcance
<!-- In: spec→migración→DTO→servicio→controller→tests→gates→tag. Out: AWS, saga, Redis, OTel full, NBomber, Chaos, Pact-real, hosting/CD. -->

## 3. Contratos API
<!-- Rutas api/v{version:apiVersion}/[controller], v1.0 estricta. Códigos 201/200/204/400/401/403/404/409/422/429/500. PagedResult{Items,TotalCount,Page,PageSize}. Enlazar docs/endpoints.md, no duplicar. -->

## 4. Modelo de datos
<!-- Tablas Prefijo* + str/int/dec/dte/bln + RowVersion rowversion + decimal(18,2). Migraciones sin _. -->

## 5. Seguridad
<!-- Validar en frontera, JWT HS256 real + Argon2id real + TOTP real, rate limits, headers §6. -->

## 6. Tests / Gates
<!-- Unit, Integration SQL real Testcontainers puerto fijo, Security, Database, Contract captura CONTRACT_CAPTURE=1. Cobertura ≥45% scripts/check_coverage.py. -->

## 7. Plan de entrega
<!-- dotnet publish -c Release -o ./publish, env requeridos, curl /health/ready + login + 401 + headers, tag vX.Y.Z-crudN. -->

## Aprobación
- [ ] Revisor 1 — fecha:
- [ ] CHECKLIST_PR.md OK
- [ ] Sin conflicto con constitución
