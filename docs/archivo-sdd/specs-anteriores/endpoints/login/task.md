# Tareas — Endpoint / flujo de login

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias entre tareas

1. T01 antes de T02
2. T02 antes de T03

## Tareas

### T01. Verificar contrato actual

- Entrada/salida.
- Códigos HTTP.

**Archivos:** spec.md

**Terminado cuando:** Contrato documentado y probado.

### T02. Cubrir validaciones y errores

- Credenciales inválidas.
- Bloqueo/rate limit.
- Red.

**Archivos:** pruebas

**Terminado cuando:** Casos de error cubiertos.

### T03. Cubrir seguridad de sesión

- Expiración.
- Almacenamiento.
- Logs.

**Archivos:** pruebas de seguridad

**Terminado cuando:** Sin secretos en logs ni respuestas.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
