# 42-endpoint-auth-recuperar-password — Endpoint de solicitud de recuperación de contraseña (EP-AU-05)

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/recuperar-password` · Código: EP-AU-05 · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Iniciar el proceso de recuperación de contraseña desde la Pantalla '¿Olvidaste tu contraseña?' (BE-03, BE-35, MO-11), generando un token de un solo uso con vigencia de 30 minutos y enviando el enlace por correo electrónico, respondiendo siempre con un mensaje genérico para evitar la enumeración de usuarios.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/recuperar-password` |
| Código Tarea | `EP-AU-05` |
| Autenticación | Público |
| Consumidor | Flutter (Móvil) y Angular (Web) |
| Estado | Propuesto |

**Parámetros / entrada:**

- Body JSON:
```json
{
  "correo": "usuario@example.com"
}
```

**Respuesta (200 OK):**
```json
{
  "mensaje": "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña."
}
```

## 3. Códigos HTTP

- 200: Operación aceptada (mensaje genérico uniforme).
- 400: Formato de correo inválido o campo vacío.
- 429: Rate limit estricto excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. `correo` obligatorio y con formato RFC válido.
2. Normalización de correo a minúsculas para búsqueda en `idx_usuarios_correo_lower`.
3. Si el usuario existe y está activo:
   - Invalida tokens de recuperación previos pendientes.
   - Genera token aleatorio criptográfico (`RandomNumberGenerator`).
   - Vigencia de 30 minutos (`expira_en = DateTime.UtcNow.AddMinutes(30)`).
   - Guarda únicamente el hash seguro del token en la base de datos (`tokens_recuperacion`).
   - Envía el correo con el enlace que incluye el token en texto claro a través del servicio de mensajería (BE-35).
4. Si el usuario NO existe: responde exactamente igual con 200 OK y el mismo mensaje sin disparar error ni envío.

## 5. Seguridad

- ASVS L2, protección estricta contra enumeración de usuarios (timing attack resistant y misma respuesta HTTP).
- Cero almacenamiento del token en texto plano.
- Rate limiting por IP y por correo.

## 6. Criterios de aceptación

- [ ] Responde 200 con el mensaje genérico tanto para correos existentes como inexistentes.
- [ ] Genera token temporal de 30 minutos y almacena su hash si el usuario existe.
- [ ] Invalida tokens de recuperación previos del usuario.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/05-autenticacion-jwt/`
- `specs-endpoints/13-endpoint-login/`
