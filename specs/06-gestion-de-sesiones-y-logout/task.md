# Tareas — 06-gestion-de-sesiones-y-logout — Gestión de sesiones y logout

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias

1. T01 antes de T02
2. T02 antes de T03

## Actividades

### T01. Verificar revocación

- jti.
- Expiración.
- Idempotencia.

**Archivos:** API_SDD/Controllers/V1/AuthController.cs, API_SDD/Services/, UnitTest/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

### T02. Documentar límites

- Blacklist en memoria no distribuida.
- Reinicio pierde revocaciones.

**Archivos:** API_SDD/Controllers/V1/AuthController.cs, API_SDD/Services/, UnitTest/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

### T03. Cubrir pruebas

- Token válido.
- Revocado.
- Sin token.

**Archivos:** API_SDD/Controllers/V1/AuthController.cs, API_SDD/Services/, UnitTest/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
