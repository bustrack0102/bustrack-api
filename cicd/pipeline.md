# Pipeline detallado — BusTrack API

## Workflows

- `../.github/workflows/ci-pr.yml`: integración continua en PR y `main`.
- Workflows de despliegue: **no existen**; Azure es propuesta.

## Etapas y comandos

```text
dotnet restore API_SDD.slnx
dotnet build API_SDD.slnx -c Release
dotnet test UnitTest/UnitTest.csproj -c Release
```

## Análisis estático

- Reglas locales en `../.semgrep/rules/semgrep.yaml`.
- El workflow debe apuntar a esa ruta; un fallo de Semgrep bloquea la fusión.

## Dependencias y secretos

- La auditoría de dependencias es bloqueante salvo vulnerabilidad justificada y registrada.
- Los secretos de CI se configuran en GitHub; nunca en el repositorio.
- Los PR no usan secretos productivos.

## Artefactos

- Resultados de pruebas y cobertura.
- No se publican artefactos con PII ni secretos.
