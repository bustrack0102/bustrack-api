# 44-endpoint-usuario-perfil-me — Endpoint de consulta de perfil de usuario autenticado (EP-AU-07)

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/usuarios/me` · Código: EP-AU-07 · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Obtener la información del perfil del usuario autenticado en la Pantalla de Perfil y Ajustes (MO-18, BE-03) a partir de los claims del token JWT, incluyendo datos básicos, rol, resumen de actividad (viajes guardados, calificaciones realizadas) y el estado del consentimiento de sensores (acelerómetro).

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/usuarios/me` |
| Código Tarea | `EP-AU-07` |
| Autenticación | Usuario / Admin (JWT requerido) |
| Consumidor | Flutter (Móvil) y Angular (Web) |
| Estado | Implementado |

**Parámetros / entrada:**

- Header: `Authorization: Bearer {accessToken}`

**Respuesta (200 OK):**
```json
{
  "id": 15,
  "nombreCompleto": "Carlos Roberto Morales",
  "correo": "carlos.morales@example.com",
  "rol": "Usuario",
  "fechaCreacion": "2026-11-01T14:20:00Z",
  "consentimientoSensoresActivo": true,
  "totalViajesGuardados": 4,
  "totalCalificaciones": 7
}
```

## 3. Códigos HTTP

- 200: Perfil obtenido exitosamente.
- 401: Token JWT ausente, expirado o revocado.
- 404: Usuario del token no encontrado en base de datos.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. Extracción segura del `userId` a partir del claim `sub` / `NameIdentifier` del token JWT.
2. Consulta puntual por clave primaria `id` sobre la tabla `usuarios`.
3. Unión con `roles` para el nombre del rol, con `consentimientos_sensores` para el estado del acelerómetro y conteo de agregados de `viajes_guardados` y `calificaciones`.
4. El usuario debe encontrarse con `activo = true`.

## 5. Seguridad

- ASVS L2, validación estricta de JWT Bearer y verificación contra blacklist de revocación.
- No se expone información sensible ni campos internos como hash de contraseña ni row version.

## 6. Criterios de aceptación

- [ ] Devuelve 200 con los datos del usuario autenticado y su resumen de actividad.
- [ ] Rechaza con 401 si no hay token o es inválido.
- [ ] No permite consultar perfiles ajenos mediante este endpoint (diseño anti-IDOR).

## 7. Dependencias

- `constitution.md`
- `specs/04-gestion-de-usuarios/`
- `specs/05-autenticacion-jwt/`
- `specs-endpoints/17-endpoint-usuario-individual/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
