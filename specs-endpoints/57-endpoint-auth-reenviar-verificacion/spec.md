# 57-endpoint-auth-reenviar-verificacion — Endpoint de reenvío de correo de verificación

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/reenviar-verificacion` · Estado: Implementado · Fecha: 2026-11-19.

## 1. Objetivo

Permitir que un usuario registrado cuya cuenta permanezca pendiente de activación solicite un nuevo enlace de verificación de correo electrónico cuando el mensaje original no haya sido recibido o el enlace haya expirado.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/reenviar-verificacion` |
| Código Tarea | `EP-AU-12` |
| Autenticación | Público (AllowAnonymous) |
| Consumidor | Flutter (Móvil) y Angular / Web |
| Estado | Implementado |

**Cuerpo JSON:**
```json
{
  "correo": "usuario@ejemplo.com"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "mensaje": "Si el correo corresponde a una cuenta pendiente de verificación, recibirás un nuevo enlace."
}
```

## 3. Códigos HTTP

- **200 OK**: Solicitud procesada (respuesta neutra genérica anti-enumeración de cuentas).
- **400 Bad Request**: Correo ausente o formato inválido.
- **429 Too Many Requests**: Límite de tasa por IP/cuenta excedido.
- **500 Internal Server Error**: Error interno no controlado.

## 4. Validaciones y Seguridad

1. Validación de formato de correo RFC 5322.
2. Respuesta neutra idéntica tanto si el correo existe como si no existe (previene ataques de enumeración).
3. Invalidación automática de cualquier token previo activo para esa cuenta.
4. Generación de nuevo token criptográfico con 24 horas de vigencia.
5. Rate limiting estricto con política "Login" (máximo 5 solicitudes por 5 minutos).

## 5. Criterios de Aceptación

- [x] Genera nuevo token y despacha correo a cuentas pendientes de verificación.
- [x] Invalida tokens anteriores vigentes del usuario.
- [x] Responde 200 OK genérico tanto para correos existentes como inexistentes.
- [x] Rechaza correos mal formados con 400 Bad Request.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-19
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md


- [x] Genera nuevo token e invalida los previos si la cuenta está pendiente.
- [x] Responde 200 OK genérico protegiendo la privacidad.
- [x] Rechaza correos con formato inválido con 400 Bad Request.
- [x] Documentado en `docs/VERIFICACION-CORREO.md` y `Memoria.md`.
