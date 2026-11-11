# 34-endpoint-auth-2fa — Endpoint de autenticación de dos factores (2FA / MFA)

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/2fa/verify` y `POST /api/v1/auth/2fa/setup` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Implementar un segundo factor de autenticación obligatorio para cuentas con permisos administrativos (`AdminPolicy`) mediante algoritmos TOTP estándar (RFC 6238), permitiendo la configuración y la verificación previa a la emisión del token con privilegios elevados.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/2fa/setup` y `/api/v1/auth/2fa/verify` |
| Autenticación | JWT provisional (pre-autenticación) en cabecera |
| Consumidor | Angular (Panel Admin) y Flutter (Móvil) |
| Estado | Propuesto |

**Endpoints específicos:**

1. `POST /api/v1/auth/2fa/setup`:
   - Genera secreto TOTP compartido y URI estándar para código QR (Authenticator Apps).
   - Retorna URI otpauth y códigos de recuperación uniuso hasheados.

2. `POST /api/v1/auth/2fa/verify`:
   - Entrada: código TOTP de 6 dígitos numéricos.
   - Salida: Token JWT definitivo con claim `amr: ["pwd", "mfa"]` y `role: Admin`.

```json
{
  "code": "482910"
}
```

**Respuesta verify:**

- 200: Código correcto, JWT definitivo emitido.
- 400: Código mal formado.
- 401: Código incorrecto, expirado o token provisional no válido.

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

## 3. Códigos HTTP

- 200: Verificación exitosa.
- 400: Parámetros inválidos.
- 401: Código TOTP incorrecto o expirado.
- 429: Rate limit estricto de intentos fallidos.
- 500: Error interno no controlado.

## 4. Validaciones

1. Código TOTP debe ser numérico de exactamente 6 dígitos.
2. Ventana de tiempo de tolerancia máxima: ±1 intervalo de 30 segundos.
3. Bloqueo temporal de cuenta ante 5 intentos consecutivos fallidos.

## 5. Seguridad

- ASVS L2, almacenamiento seguro de la clave secreta TOTP cifrada en reposo.
- Protección contra ataques de fuerza bruta con rate limit estricto.

## 6. Criterios de aceptación

- [ ] Genera secreto TOTP y valida códigos temporales según RFC 6238.
- [ ] Emite JWT con claims de 2FA verificado.
- [ ] Bloquea intentos repetidos fallidos.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/05-autenticacion-jwt/`
- `specs-endpoints/13-endpoint-login/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
