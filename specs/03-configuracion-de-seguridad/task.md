# Tareas — 03-configuracion-de-seguridad — Configuración de seguridad

> Spec: `spec.md` · Plan: `plan.md` · Estado: Parcial.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias

1. T01 antes de T02
2. T02 antes de T03

## Actividades

### T01. Verificar hashing y JWT

- Parámetros Argon2id.
- MapInboundClaims.
- Expiración.

**Archivos:** API_SDD/Security/, API_SDD/Logging/, docs/asvs-l2-checklist.md, .semgrep/rules/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

### T02. Revisar secretos y logs

- Origen de claves.
- Scrubbing.
- Sin secretos en Git.

**Archivos:** API_SDD/Security/, API_SDD/Logging/, docs/asvs-l2-checklist.md, .semgrep/rules/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

### T03. Actualizar checklist ASVS

- Controles aplicables.
- Brechas.

**Archivos:** API_SDD/Security/, API_SDD/Logging/, docs/asvs-l2-checklist.md, .semgrep/rules/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
