using System.Text.Json.Serialization;

namespace ApiSDD.Dtos;

// EP-AU-01: Registro de usuario
public sealed record RegistroRequestDto(
    string nombreCompleto,
    string correo,
    string password,
    string confirmacionPassword,
    bool aceptaAvisoPrivacidad
);

public sealed record UsuarioPerfilResumenDto(
    Guid id,
    string nombreCompleto,
    string correo,
    string rol
);

public sealed record AuthResponseDto(
    UsuarioPerfilResumenDto usuario,
    string accessToken,
    string refreshToken,
    int expiraEnSegundos
);

// EP-AU-02: Login unificado
public sealed record LoginInputDto(
    string? correo,
    string? password,
    string? strNombreUsuario,
    string? strPWD,
    string? dispositivoInfo
);

// EP-AU-03: Refresh Token
public sealed record RefreshTokenRequestDto(
    string refreshToken
);

public sealed record TokenRefreshResponseDto(
    string accessToken,
    string refreshToken,
    int expiraEnSegundos
);

// EP-AU-04: Logout
public sealed record LogoutRequestDto(
    string? refreshToken,
    bool cerrarTodosLosDispositivos = false
);

// EP-AU-05: Recuperación de contraseña
public sealed record RecuperarPasswordRequestDto(
    string correo
);

// EP-AU-06: Restablecimiento de contraseña
public sealed record RestablecerPasswordRequestDto(
    string token,
    string nuevaPassword,
    string confirmacionPassword
);

// Respuestas genéricas de mensaje
public sealed record MessageResponseDto(
    string mensaje
);
