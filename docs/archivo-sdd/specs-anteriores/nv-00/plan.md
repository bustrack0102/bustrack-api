# plan.md — NV-00 Constitución MVP

1. Mapeo
- Crear: `specs/nv-00/spec.md` ya existente + `AGENTS.md §1 recortado + Memoria.md inicial + appsettings.Example.json mínimo + Directory.Build.props base`.
- Modificar: ninguno, es primera spec. Si existe `AGENTS.md` madre, solo citarla no copiarla.

2. Guardarraíles
- `critic B1` secretos `key/password/secret/connectionstring` en Git → FAIL.
- `critic B2` `token` en logs/errores → FAIL, única excepción futura `blacklist:{jti}`.
- `S1135` caza `todo` incluso minúsculas → prohibido sin issue.
- `CA1707` migraciones sin `_`.
- `TreatWarningsAsErrors=true + AnalysisMode=All + NuGetAudit=true` desde día 1.

3. Pruebas
- `UnitTest/Common/AppSettingsTests.cs (3)`: `Example.json válido + sin patrones sensibles + sin claves futuras Sqs/Transport/Redis/PERF_*`.
- Manual: `AGENTS.md §1 cita stack + Memoria.md existe + cada NFR tiene comando`.

4. Secuencia
1. Fijar stack recortado en `spec.md` tabla por capa.
2. Redactar gobernanza SDD + DoD por slice + trazabilidad + 1 revisor.
3. Fijar NFR MVP con comando: `timeout 60s, Kestrel 10MB/2min, build 0/0, coverage >=45%`.
4. Registrar 6 reglas no negociables + `real-first sin fakes + IOptionsMonitor lazy`.
5. Crear `AGENTS.md §1 + Memoria.md + Example.json mínimo` y verificar gates en vacío.

5. Verificación
- `dotnet build -c Release --no-restore → 0/0`
- `dotnet test UnitTest -c Release --no-build → AppSettingsTests 3/3`
- `powershell -File scripts/critic-guardrails.ps1 → PASS exit 0`
