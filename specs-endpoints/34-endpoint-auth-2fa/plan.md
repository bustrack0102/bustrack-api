# Plan técnico — 34-endpoint-auth-2fa — Endpoint de autenticación de dos factores (2FA / MFA)

> Spec: `spec.md` · Endpoint: `POST /api/v1/auth/2fa/*` · Estado: Propuesto.

## 1. Estrategia

Diseñar el flujo de dos factores con claims en JWT para diferenciar sesiones parciales (`pre-auth`) de sesiones con segundo factor verificado (`mfa_complete`), y validar tokens TOTP con `OtpNet`.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, RFC 6238 TOTP.
- Componentes: `Auth2FaController`, `ITotpService`, `Verify2FaRequestDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/Auth2FaController.cs
API_SDD/Dtos/Auth/Verify2FaRequestDto.cs
API_SDD/Security/ITotpService.cs
specs/05-autenticacion-jwt/
```

## 4. Seguridad

- ASVS L2, cifrado de claves TOTP simétricas.
- Rate limiting por usuario e IP.

## 5. Errores

- Retorno de 401 ProblemDetails en código erróneo sin detallar si la falla es por tiempo o por clave.

## 6. Pruebas

- Pruebas unitarias de cálculo TOTP dentro y fuera de ventana.
- Pruebas de integración de flujo de login parcial a emisión de token definitivo.

## 7. Riesgos

- Desincronización de reloj mitigada con tolerancia de un paso previo y posterior.

