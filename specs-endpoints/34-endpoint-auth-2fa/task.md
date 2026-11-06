# Tareas — 34-endpoint-auth-2fa — Endpoint de autenticación de dos factores (2FA / MFA)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar servicio TOTP y endpoints

- Implementar `ITotpService` para generación y verificación de claves secretas.
- Definir endpoints de configuración (`/setup`) y verificación (`/verify`).

**Archivos:** API_SDD/Security/ITotpService.cs, API_SDD/Controllers/V1/Auth2FaController.cs

**Terminado cuando:** el endpoint de verificación reciba el código de 6 dígitos y valide su autenticidad.

### T02. Pruebas de validación y límites

- Validar éxito con código TOTP actual.
- Validar rechazo con código vencido o modificado.
- Validar bloqueo tras intentos fallidos consecutivos.

**Archivos:** UnitTest/Security/TotpTests.cs

**Terminado cuando:** todas las pruebas de seguridad de segundo factor pasen en verde.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

