# Tasks Template — SDD

> Cada CRUD liberable: spec→migración→DTO→servicio→controller→tests→gates→tag.

- [ ] 1. Spec en `specs/NV-0X/spec.md` aprobada
- [ ] 2. Migración EF sin `_` + SQL Server externo
- [ ] 3. DTO + FluentValidation + `PropertyNamingPolicy=null` legacy medida
- [ ] 4. Servicio DI (`IOptionsMonitor` lazy si override WAF), sin `new` clave, `Reset()+lock` si estático
- [ ] 5. Controller `api/v{version:apiVersion}` sin try/catch, mapeo 404/403/409/422
- [ ] 6. Tests: Unit + Integration real + Security + Database + Contract captura
- [ ] 7. Gates: cobertura ≥45% + `endpoints.md` + `rate-limit-matrix.md` + `asvs-l2-checklist.md` vivos
- [ ] 8. `dotnet publish -c Release -o ./publish` + curl `/health/ready` + tag `vX.Y.Z-crudN`
- [ ] 9. Actualizar `Memoria.md` + `CHECKLIST_PR.md`
