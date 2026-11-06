# Tareas — 33-endpoint-auth-refresh — Endpoint de refresco y renovación de tokens JWT

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Definir esquemas y servicio de tokens

- Crear `RefreshTokenRequestDto` y `RefreshTokenResponseDto`.
- Definir interfaz para validar, rotar y revocar refresh tokens.

**Archivos:** API_SDD/Dtos/Auth/RefreshTokenRequestDto.cs, API_SDD/Security/ITokenService.cs

**Terminado cuando:** los DTOs y la interfaz del servicio de tokens estén definidos.

### T02. Pruebas de expiración y rotación

- Validar éxito con refresh token vigente y emisión de nuevo token.
- Validar revocación automática ante detección de reuso de token anterior.

**Archivos:** UnitTest/Security/TokenRefreshTests.cs

**Terminado cuando:** las pruebas unitarias y de integración confirmen la seguridad de rotación.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

