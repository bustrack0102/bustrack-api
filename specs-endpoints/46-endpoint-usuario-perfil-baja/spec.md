# 46-endpoint-usuario-perfil-baja — Endpoint de baja de cuenta de usuario autenticado (EP-AU-09)

> Repositorio: BusTrack API · Endpoint: `DELETE /api/v1/usuarios/me` · Código: EP-AU-09 · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Ejecutar la baja definitiva y revocación de la cuenta del usuario autenticado desde la opción 'Eliminar mi cuenta' en la pantalla de Perfil y Ajustes (MO-18, BE-40) en cumplimiento del aviso de privacidad y derechos ARCO, requiriendo confirmación de contraseña, desactivando el acceso, revocando todas sus sesiones activas, eliminando sus viajes guardados y anonimizando sus datos personales para salvaguardar la integridad de las calificaciones históricas.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `DELETE` |
| Ruta | `/api/v1/usuarios/me` |
| Código Tarea | `EP-AU-09` |
| Autenticación | Usuario (JWT requerido) |
| Consumidor | Flutter (Móvil) y Angular (Web) |
| Estado | Propuesto |

**Parámetros / entrada:**

- Header: `Authorization: Bearer {accessToken}`
- Body JSON:
```json
{
  "passwordConfirmacion": "Password123!"
}
```

**Respuesta (200 OK):**
```json
{
  "mensaje": "Tu cuenta y datos personales han sido dados de baja correctamente."
}
```

## 3. Códigos HTTP

- 200: Cuenta dada de baja y datos anonimizados.
- 400: Contraseña de confirmación incorrecta o ausente.
- 401: Token JWT ausente, expirado o revocado.
- 429: Rate limit de operaciones críticas.
- 500: Error interno no controlado.

## 4. Validaciones

1. `passwordConfirmacion` obligatoria; se compara contra el hash almacenado mediante Argon2id.
2. Identificador extraído de forma inmutable del token JWT (`userId`).
3. Transacción atómica de baja:
   - Desactiva el usuario (`activo = false`, `fecha_baja = DateTime.UtcNow`).
   - Anonimiza datos personales PII (`nombreCompleto = 'Usuario Anonimizado'`, `correo = 'anonimo_{id}@bustrack.local'`).
   - Elimina registros dependientes en `refresh_tokens`, `tokens_recuperacion` y `viajes_guardados` (vía FK con borrado en cascada).
   - Preserva los registros en `calificaciones` desvinculando la identidad de la persona para no romper las estadísticas de calidad del servicio.

## 5. Seguridad

- ASVS L2, exigencia de re-autenticación obligatoria con contraseña antes de ejecutar la baja.
- Anonimización inmediata para cumplimiento estricto de privacidad.

## 6. Criterios de aceptación

- [ ] Requiere y valida la contraseña actual antes de dar de baja la cuenta.
- [ ] Invalida de forma irrevocable todas las sesiones del usuario en `refresh_tokens`.
- [ ] Anonimiza datos de perfil y conserva integridad en calificaciones.
- [ ] Rechaza con 400 si la contraseña de confirmación no coincide.

## 7. Dependencias

- `constitution.md`
- `specs/04-gestion-de-usuarios/`
- `specs-endpoints/14-endpoint-logout/`
- `specs-endpoints/44-endpoint-usuario-perfil-me/`
