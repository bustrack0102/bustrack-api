# 56-endpoint-auth-verificar-correo — Endpoint de verificación de correo electrónico

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/auth/verificar-correo` · Estado: Implementado · Fecha: 2026-11-19.

## 1. Objetivo

Validar el token criptográfico recibido mediante el enlace enviado al correo electrónico del usuario, activando su cuenta (`Activo = true`, `CorreoVerificado = true`) e invalidando de forma segura el token temporal de un solo uso.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/auth/verificar-correo` |
| Código Tarea | `EP-AU-11` |
| Autenticación | Público (AllowAnonymous) |
| Consumidor | Flutter (Móvil) y Angular / Web Browser |
| Estado | Implementado |

**Parámetros / Query String:**
- `token` (string, obligatorio): Token criptográfico hexadecimal generado al momento del registro o reenvío.

**Respuesta exitosa (200 OK):**
```json
{
  "verificado": true,
  "mensaje": "¡Correo verificado con éxito! Tu cuenta ha sido activada y ya puedes iniciar sesión en BusTrack."
}
```

## 3. Códigos HTTP

- **200 OK**: Correo verificado satisfactoriamente o previamente verificado.
- **400 Bad Request**: Token ausente, inválido, expirado (más de 24 horas) o ya utilizado.
- **429 Too Many Requests**: Límite de tasa excedido por política de rate limiting.
- **500 Internal Server Error**: Error interno no controlado.

## 4. Validaciones de Seguridad

1. Token obligatorio con entropía criptográfica (32 bytes / 64 caracteres hex).
2. Verificación de hash SHA-256 en la base de datos (nunca se almacena en texto claro).
3. Expiración estricta de 24 horas (`FechaExpiracion`).
4. Token de un solo uso: se marca `Usado = true` y se registra `FechaUso`.
5. No emite JWT ni credenciales de sesión en la respuesta para evitar secuestro de enlaces en clientes de correo.

## 5. Criterios de Aceptación

- [x] Valida tokens válidos y activa la cuenta.
- [x] Rechaza tokens vencidos, inexistentes o reutilizados con 400 Bad Request.
- [x] Soporta persistencia dual SQL Server y MongoDB.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-19
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md

- [x] Documentado en `docs/VERIFICACION-CORREO.md` y `Memoria.md`.
