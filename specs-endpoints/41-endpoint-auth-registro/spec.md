# 41-endpoint-auth-registro — Endpoint de registro de usuario (EP-AU-01)

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/auth/registro` · Código: EP-AU-01 · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Registrar una nueva cuenta de usuario final en BusTrack desde la Pantalla de Registro (HU-01, MO-11), asignando el rol predeterminado 'Usuario' y emitiendo de inmediato el primer par de tokens (JWT de acceso y Refresh Token) para iniciar sesión tras el registro.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/auth/registro` |
| Código Tarea | `EP-AU-01` |
| Autenticación | Público |
| Consumidor | Flutter (Móvil) y Angular (Web) |
| Estado | Implementado |

**Parámetros / entrada:**

- Body JSON:
```json
{
  "nombreCompleto": "Carlos Roberto Morales",
  "correo": "carlos.morales@example.com",
  "password": "Password123!",
  "confirmacionPassword": "Password123!",
  "aceptaAvisoPrivacidad": true
}
```

**Respuesta (201 Created):**
```json
{
  "usuario": {
    "id": 15,
    "nombreCompleto": "Carlos Roberto Morales",
    "correo": "carlos.morales@example.com",
    "rol": "Usuario"
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "d8e8fca2dc6b4bf09c48b2a3fa42e1bb45...",
  "expiraEnSegundos": 3600
}
```

## 3. Códigos HTTP

- 201: Creado con éxito, tokens de sesión emitidos.
- 400: Datos inválidos, contraseñas no coinciden o `aceptaAvisoPrivacidad` es falso.
- 409: Conflicto, correo electrónico ya registrado (insensible a mayúsculas/minúsculas).
- 422: Contraseña débil o formato de correo erróneo.
- 429: Rate limit de peticiones excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. `nombreCompleto`, `correo`, `password`, `confirmacionPassword` obligatorios.
2. `aceptaAvisoPrivacidad` debe ser `true` estrictamente.
3. Formato de correo válido y verificación de unicidad contra índice de correo en minúsculas (`idx_usuarios_correo_lower`).
4. Contraseña de mínimo 8 caracteres con letras y números; `confirmacionPassword` idéntico.
5. Algoritmo de hash con sal Argon2id (`IPasswordHasher`).
6. Asignación automática de rol 'Usuario' (`rol_id`).

## 5. Seguridad

- ASVS L2, Argon2id, rate limiting por IP para evitar creación masiva de cuentas fantasma.
- Cero almacenamiento de contraseñas en texto claro.
- Refresh token almacenado con hash criptográfico.

## 6. Criterios de aceptación

- [ ] Registra al usuario y devuelve 201 Created con usuario y par de tokens.
- [ ] Rechaza con 409 Conflict si el correo ya existe.
- [ ] Rechaza con 400 si las contraseñas no coinciden o no se acepta el aviso de privacidad.
- [ ] `Memoria.md` refleja el estado resultante.

## 7. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `specs/04-gestion-de-usuarios/`
- `specs/05-autenticacion-jwt/`
- `specs-endpoints/13-endpoint-login/`
