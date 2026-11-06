# 45-endpoint-usuario-perfil-editar — Endpoint de edición de perfil de usuario autenticado (EP-AU-08)

> Repositorio: BusTrack API · Endpoint: `PUT /api/v1/usuarios/me` · Código: EP-AU-08 · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Permitir al usuario autenticado actualizar los datos de su propio perfil desde la Pantalla Editar Perfil en Ajustes (MO-18, BE-03), modificando su nombre completo o correo electrónico, y permitiendo opcionalmente el cambio de contraseña suministrando su clave actual.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `PUT` |
| Ruta | `/api/v1/usuarios/me` |
| Código Tarea | `EP-AU-08` |
| Autenticación | Usuario / Admin (JWT requerido) |
| Consumidor | Flutter (Móvil) y Angular (Web) |
| Estado | Propuesto |

**Parámetros / entrada:**

- Header: `Authorization: Bearer {accessToken}`
- Body JSON:
```json
{
  "nombreCompleto": "Carlos R. Morales G.",
  "correo": "carlos.morales.nuevo@example.com",
  "passwordActual": "Password123!",
  "nuevaPassword": "NuevoPasswordSeguro456!"
}
```

**Respuesta (200 OK):**
```json
{
  "id": 15,
  "nombreCompleto": "Carlos R. Morales G.",
  "correo": "carlos.morales.nuevo@example.com",
  "fechaActualizacion": "2026-11-05T19:00:00Z"
}
```

## 3. Códigos HTTP

- 200: Perfil actualizado correctamente.
- 400: Datos de entrada inválidos o `passwordActual` incorrecta si se solicita cambio de contraseña.
- 401: Token ausente, expirado o revocado.
- 409: Conflicto, el nuevo correo ya pertenece a otra cuenta.
- 422: Nueva contraseña no cumple con las políticas de complejidad.
- 429: Rate limit de escrituras concurrentes.
- 500: Error interno no controlado.

## 4. Validaciones

1. `userId` obtenido de los claims del token JWT (anti-IDOR).
2. Si se modifica `correo`, verificar que no exista otra cuenta con el mismo correo en minúsculas (`idx_usuarios_correo_lower`) con `id != userId`.
3. Si se incluye `nuevaPassword`:
   - `passwordActual` es obligatoria y se verifica contra el hash actual con Argon2id.
   - `nuevaPassword` debe cumplir política de seguridad (mínimo 8 caracteres con letras y números).
   - Se actualiza el hash de contraseña y se revocan las demás sesiones activas.

## 5. Seguridad

- ASVS L2, Argon2id, verificación estricta de la contraseña actual antes de permitir cualquier mutación de credenciales.
- No permite escalar privilegios: el campo `rol` no es modificable a través de este endpoint.

## 6. Criterios de aceptación

- [ ] Actualiza nombre y correo del usuario autenticado.
- [ ] Valida unicidad del nuevo correo rechazando con 409 si ya existe.
- [ ] Requiere y valida `passwordActual` si se envía `nuevaPassword`.

## 7. Dependencias

- `constitution.md`
- `specs/04-gestion-de-usuarios/`
- `specs/05-autenticacion-jwt/`
- `specs-endpoints/44-endpoint-usuario-perfil-me/`
