# Sistema de Verificación de Correo Electrónico — BusTrack API

> Documentación de arquitectura, endpoints, persistencia dual y guía de integración para clientes móviles (Flutter) y web.

---

## 1. Visión General

El sistema de verificación de correo electrónico en **BusTrack API** agrega un mecanismo seguro, asíncrono y aditivo para validar que los usuarios registrados sean propietarios legítimos de las direcciones de correo proporcionadas antes de permitir el acceso a recursos protegidos y generación de sesiones autenticadas.

### Características Principales
- **Generación Criptográfica de Tokens:** Generación con `RandomNumberGenerator.GetBytes(64)` en formato Base64Url (sin caracteres especiales).
- **Almacenamiento Seguro (One-Way Hashing):** En la base de datos se almacena exclusivamente el hash SHA-256 en hexadecimal del token (`TokenHash`). El token en texto claro jamás se persiste y solo viaja en el enlace enviado al buzón del usuario.
- **Inactivación de Tokens Anteriores:** Cada emisión de un nuevo token para el mismo usuario invalida automáticamente cualquier token pendiente previo (`Usado = true`).
- **Ventana de Expiración:** Cada token tiene una vigencia configurable (por defecto 24 horas).
- **Protección contra Enumeración:** Los endpoints de registro y reenvío de correo responden de manera homogénea y segura para evitar inferir la existencia previa de cuentas.
- **Doble Persistencia:** Compatibilidad total con SQL Server (EF Core) y MongoDB NoSQL (`IBusTrackNoSqlStore`).
- **Retrocompatibilidad:** Las cuentas creadas con anterioridad que ya estaban activadas (`Activo = true`) conservan su acceso sin verse afectadas.

---

## 2. Endpoints de Verificación de Correo

### 2.1. GET `/api/v1/auth/verificar-correo` (EP-AU-11)
Valida el token enviado por correo electrónico y activa la cuenta del usuario.

- **Método:** `GET`
- **Ruta:** `/api/v1/auth/verificar-correo?token={token}`
- **Autenticación:** Pública (No requiere JWT).
- **Rate Limit:** Política `Auth` (5 req / 1 min por IP).
- **Parámetros Query:**
  - `token` (string, obligatorio): Token Base64Url recibido en el correo.

#### Respuestas:
- **`200 OK`**: Correo verificado satisfactoriamente.
  ```json
  {
    "verificado": true,
    "mensaje": "Correo electrónico verificado exitosamente. Ahora puedes iniciar sesión en BusTrack."
  }
  ```
- **`400 Bad Request`**: Token ausente, expirado, inválido o previamente utilizado.
  ```json
  {
    "verificado": false,
    "mensaje": "El token de verificación es inválido o ha expirado. Solicita un nuevo enlace de verificación."
  }
  ```

---

### 2.2. POST `/api/v1/auth/reenviar-verificacion` (EP-AU-12)
Permite solicitar un nuevo correo con enlace de verificación en caso de expiración o extravío.

- **Método:** `POST`
- **Ruta:** `/api/v1/auth/reenviar-verificacion`
- **Autenticación:** Pública (No requiere JWT).
- **Rate Limit:** Política `Auth` (5 req / 1 min por IP).
- **Body JSON:**
  ```json
  {
    "correo": "usuario@ejemplo.com"
  }
  ```

#### Respuestas:
- **`200 OK`**: Respuesta genérica anti-enumeración (se devuelve tanto si el usuario existe como si no existe o ya está verificado).
  ```json
  {
    "enviado": true,
    "mensaje": "Si el correo existe en nuestro sistema y está pendiente de verificación, se ha enviado un enlace de confirmación."
  }
  ```
- **`400 Bad Request`**: Correo electrónico mal formado o vacío.

---

## 3. Modificaciones en el Flujo de Registro e Inicio de Sesión

### 3.1. Registro de Usuario (`POST /api/v1/auth/registro`)
- Al registrarse, el usuario se crea con:
  - `Activo = false`
  - `CorreoVerificado = false`
  - `FechaVerificacionCorreo = null`
- Se genera un token criptográfico de 64 bytes y se almacena su hash SHA-256 en la tabla/colección `tokens_verificacion_correo`.
- Se despacha un correo HTML responsive con el enlace directo de verificación.
- La respuesta HTTP es `201 Created`, pero **no** entrega tokens de acceso (`accessToken: null`, `refreshToken: null`):
  ```json
  {
    "usuario": {
      "id": 105,
      "nombreCompleto": "Carlos Mendoza",
      "correo": "carlos.mendoza@ejemplo.com",
      "rol": "Usuario"
    },
    "accessToken": null,
    "refreshToken": null,
    "expiraEnSegundos": 0,
    "requiereVerificacion": true,
    "mensaje": "Cuenta registrada exitosamente. Hemos enviado un enlace de verificación a tu correo electrónico."
  }
  ```

### 3.2. Inicio de Sesión (`POST /api/v1/auth/login`)
- Si un usuario no verificado (`Activo = false` y `CorreoVerificado = false`) intenta iniciar sesión, el sistema deniega el acceso con `HTTP 400 Bad Request`:
  ```json
  {
    "error": "Debes verificar tu dirección de correo electrónico antes de iniciar sesión. Por favor revisa tu buzón o solicita un nuevo enlace."
  }
  ```

---

## 4. Persistencia Dual

### 4.1. SQL Server / EF Core
- **Entidad:** `API_SDD.Data.Entities.TokenVerificacionCorreo`
- **Tabla:** `tokens_verificacion_correo`
  - `id` (int, PK, Identity)
  - `usuario_id` (int, FK a `usuarios(id)`)
  - `token_hash` (varchar(128), Index `idx_tokens_verif_hash`)
  - `fecha_creacion` (datetime2)
  - `fecha_expiracion` (datetime2)
  - `usado` (bit)
  - `fecha_uso` (datetime2, nullable)
  - Índice compuesto: `idx_tokens_verif_usuario_vigentes` en `(usuario_id, fecha_expiracion, usado)`
- **Modificación en `usuarios`:**
  - `correo_verificado` (bit, DEFAULT 0)
  - `fecha_verificacion_correo` (datetime2, NULL)

### 4.2. MongoDB NoSQL
- **Documento:** `TokenVerificacionCorreoDoc` en colección `tokens_verificacion_correo`.
- **Colección `usuarios`:** Campos agregados `CorreoVerificado` (bool) y `FechaVerificacionCorreo` (DateTime?).
- **Métodos en `IBusTrackNoSqlStore`:**
  - `GetVerificationTokenByHashAsync(string tokenHash, CancellationToken ct)`
  - `InvalidateActiveVerificationTokensAsync(int usuarioId, CancellationToken ct)`
  - `CreateVerificationTokenAsync(TokenVerificacionCorreoDoc doc, CancellationToken ct)`
  - `UpdateVerificationTokenAsync(TokenVerificacionCorreoDoc doc, CancellationToken ct)`

---

## 5. Configuración de Correo Electrónico y Variables de Entorno

El servicio utiliza `IEmailNotificationService` e inyecta la configuración desde `appsettings.json` o variables de entorno:

```json
"Email": {
  "SmtpHost": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "SmtpUser": "apikey",
  "SmtpPass": "SG.xxxxxxxxxxxx",
  "FromEmail": "no-reply@bustrack.lat",
  "FromName": "BusTrack Soporte",
  "EnableSsl": true,
  "VerificationBaseUrl": "https://api.bustrack.lat/api/v1/auth/verificar-correo"
}
```

Variables de entorno equivalentes:
- `Email__SmtpHost`
- `Email__SmtpPort`
- `Email__SmtpUser`
- `Email__SmtpPass`
- `Email__FromEmail`
- `Email__FromName`
- `Email__EnableSsl`
- `Email__VerificationBaseUrl`

---

## 6. Guía de Integración con el Cliente Móvil (Flutter / Dart)

### 6.1. Manejo en Pantalla de Registro
Al recibir respuesta `201 Created` con `requiereVerificacion: true`:
1. No guardar tokens de autenticación (vienen en `null`).
2. Redirigir a la pantalla informativa: `PantallaVerificacionPendiente`.
3. Ofrecer un botón para abrir el cliente de correo (`url_launcher` con `mailto:`) y un botón para **Reenviar correo de verificación**.

```dart
// Ejemplo en Flutter Service
Future<AuthResult> registrarUsuario(RegistroRequest req) async {
  final response = await http.post(
    Uri.parse('$baseUrl/api/v1/auth/registro'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode(req.toJson()),
  );

  if (response.statusCode == 201) {
    final data = jsonDecode(response.body);
    if (data['requiereVerificacion'] == true) {
      return AuthResult.pendienteVerificacion(
        mensaje: data['mensaje'] ?? 'Verifica tu correo electrónico.',
      );
    }
    return AuthResult.exito(data);
  } else {
    throw Exception('Error en registro: ${response.body}');
  }
}
```

### 6.2. Reenvío de Verificación desde la App
```dart
Future<bool> reenviarVerificacion(String correo) async {
  final response = await http.post(
    Uri.parse('$baseUrl/api/v1/auth/reenviar-verificacion'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({'correo': correo}),
  );

  return response.statusCode == 200;
}
```

### 6.3. Deep Linking (Opcional para App Móvil)
Si se configura un scheme (`bustrack://verify?token=...`) o Android App Links / iOS Universal Links:
- La app puede capturar el enlace de verificación y consumir `GET /api/v1/auth/verificar-correo?token={token}` directamente mediante HTTP y mostrar confirmación nativa.
