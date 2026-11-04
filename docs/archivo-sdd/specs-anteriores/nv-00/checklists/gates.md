# Checklist — nv-00 Constitución MVP

> Trazabilidad: `spec R1-R4 ↔ plan §3-§5 ↔ task T1`. Estado: Borrador con evidencia.

- [x] `specs/nv-00/spec.md` existe con Contexto/Requisitos/Diseño/Contratos/Tests/Criterios/Límites/Aprobación
- [x] Stack recortado fijado (R1): sin SQS/Redis/OTel/NBomber/AWS/hosting
- [x] Gobernanza SDD + DoD + trazabilidad + 1 revisor (R2)
- [x] NFR con comando: build 0/0, coverage ≥45% (R3)
- [x] 6 reglas no negociables + real-first + IOptionsMonitor lazy (R4)
- [x] `AGENTS.md §1` cita stack + `Memoria.md` existe
- [x] `API_SDD/appsettings.Example.json` solo claves usadas, sin secretos
- [x] `Directory.Build.props`: All + TreatWarningsAsErrors + NuGetAudit + SonarAnalyzer
- [x] `UnitTest/Common/AppSettingsTests.cs (3)` verdes
- [x] `critic-guardrails.ps1 → PASS` en vacío
- [x] Aprobación firmada: Usuario (aceptación expresa), 2026-10-11
