namespace ApiSDD.Dtos;

// EP-AU-07: Perfil de usuario autenticado
public sealed record UsuarioMeResponseDto(
    Guid id,
    string nombreCompleto,
    string correo,
    string rol,
    DateTime fechaCreacion,
    bool consentimientoSensoresActivo,
    int totalViajesGuardados,
    int totalCalificaciones
);

// EP-AU-08: Edición de perfil
public sealed record UsuarioMeUpdateRequestDto(
    string nombreCompleto,
    string correo,
    string? passwordActual,
    string? nuevaPassword
);

public sealed record UsuarioMeUpdateResponseDto(
    Guid id,
    string nombreCompleto,
    string correo,
    DateTime? fechaActualizacion
);

// EP-AU-09: Baja de cuenta
public sealed record UsuarioMeDeleteRequestDto(
    string passwordConfirmacion
);

// EP-AU-10: Gestión de usuarios en panel de administración
public sealed record UsuarioAdminItemDto(
    Guid id,
    string nombreCompleto,
    string correo,
    string rol,
    bool activo,
    DateTime? ultimoAcceso
);

public sealed record PagedUsuariosAdminResponseDto(
    int totalRegistros,
    int paginaActual,
    int totalPaginas,
    IReadOnlyList<UsuarioAdminItemDto> items
);
