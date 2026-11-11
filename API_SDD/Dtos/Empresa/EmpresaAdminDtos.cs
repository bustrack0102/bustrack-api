namespace ApiSDD.Dtos.Empresa;

// Respuesta pública de escritura admin (no expone la entidad EF ni FuenteDatoId)
public sealed record EmpresaResponseDto(
    Guid id,
    string nombre,
    string? siglas,
    string? descripcion,
    string? municipioBase,
    string? telefonoContacto,
    string? sitioWeb,
    string? logoUrl,
    decimal calificacionPromedio,
    int totalEvaluaciones,
    bool activo,
    DateTime? fechaActualizacion
);

// EP-EM-06: Alta de Empresa
public sealed record EmpresaCreateDto(
    string nombre,
    string? siglas,
    string? descripcion,
    string? municipioBase,
    string? telefonoContacto,
    string? sitioWeb,
    string? logoUrl,
    Guid? fuenteDatoId
);

// EP-EM-07: Edición de Empresa
public sealed record EmpresaUpdateDto(
    string nombre,
    string? siglas,
    string? descripcion,
    string? municipioBase,
    string? telefonoContacto,
    string? sitioWeb,
    string? logoUrl,
    Guid? fuenteDatoId,
    bool activo
);

// EP-EM-04: Rutas de la Empresa
public sealed record EmpresaRutaItemDto(
    Guid rutaId,
    string codigo,
    string nombre,
    string medioTransporte,
    string? paradaOrigen,
    string? paradaDestino,
    decimal tarifaBaseMxn,
    string? primerHorario,
    string? ultimoHorario,
    int frecuenciaMinutos
);

// EP-EM-05: Comparador de Empresas
public sealed record EmpresaComparadaItemDto(
    Guid empresaId,
    string nombre,
    decimal calificacionPromedio,
    int totalEvaluaciones,
    IReadOnlyList<string> mediosTransporte,
    decimal tarifaPromedioMxn,
    int frecuenciaMinutos,
    string? primerSalida,
    string? ultimaSalida,
    IReadOnlyList<string> rutasDisponibles
);

public sealed record EmpresaCompararResponseDto(
    string trayecto,
    IReadOnlyList<EmpresaComparadaItemDto> empresasComparadas
);
