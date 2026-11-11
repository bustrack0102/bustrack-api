# 35-endpoint-admin-bootstrap — Endpoint de bootstrap inicial de administrador

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/admin/bootstrap` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Permitir la inicialización segura y única (One-time Setup) del primer usuario administrador del sistema en bases de datos vacías, bloqueando permanentemente su acceso posterior una vez registrado el primer administrador.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/admin/bootstrap` |
| Autenticación | Llave de bootstrap en header `X-Bootstrap-Key` (configurada en variables de entorno o Key Vault) |
| Consumidor | Script de inicialización de infraestructura o CLI de despliegue |
| Estado | Propuesto |

**Parámetros / entrada:**

- Header: `X-Bootstrap-Key: <clave-segura-de-un-solo-uso>`
- Body JSON:

```json
{
  "username": "superadmin",
  "email": "admin@bustrack.gob.gt",
  "password": "PasswordComplejo123!",
  "nombreCompleto": "Administrador Principal del Sistema"
}
```

**Respuesta:**

- 201: Administrador inicial creado exitosamente.
- 400: Validación fallida en contraseña o campos requeridos.
- 401/403: `X-Bootstrap-Key` ausente o incorrecta.
- 409: Conflicto, el sistema ya cuenta con al menos un administrador y el bootstrap queda permanentemente bloqueado.

## 3. Códigos HTTP

- 201: Creado.
- 400: Datos de usuario inválidos según política de contraseñas.
- 401/403: Llave de bootstrap incorrecta.
- 409: El sistema ya ha sido inicializado previamente.
- 429: Rate limit estricto.
- 500: Error interno no controlado.

## 4. Validaciones

1. El sistema verifica si `count(Usuarios con Rol Admin) == 0`. Si es mayor que cero, rechaza de inmediato con 409.
2. Contraseña validada con reglas estrictas (longitud mínima 12 caracteres, mayúsculas, minúsculas, números y símbolos).
3. Hash generado con Argon2id de alta resistencia.

## 5. Seguridad

- ASVS L2, clave de bootstrap en variable de entorno secreta, nunca en código.
- Bloqueo permanente e irreversible tras la creación del primer administrador.

## 6. Criterios de aceptación

- [ ] Crea el primer usuario administrador con hash Argon2id si no existe ninguno.
- [ ] Rechaza con 409 si ya existe un usuario con rol Admin.
- [ ] Rechaza con 401 si la cabecera `X-Bootstrap-Key` no coincide con la configuración del servidor.

## 7. Dependencias

- `constitution.md`
- `specs/04-gestion-de-usuarios/`
- `specs-endpoints/16-endpoint-usuarios-coleccion/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
