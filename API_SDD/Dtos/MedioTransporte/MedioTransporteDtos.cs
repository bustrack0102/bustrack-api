namespace ApiSDD.Dtos.MedioTransporte;

// EP-EM-09: Catálogo de Medios de Transporte
public sealed record MedioTransporteItemDto(
    int id,
    string clave,
    string nombre,
    string? descripcion,
    string? iconoClave,
    int totalRutasAsociadas,
    DateTime? fechaActualizacion
);

// EP-EM-10: Alta Medio de Transporte
public sealed record MedioTransporteCreateDto(
    string clave,
    string nombre,
    string? descripcion,
    string? iconoClave
);

// EP-EM-11: Edición Medio de Transporte
public sealed record MedioTransporteUpdateDto(
    string clave,
    string nombre,
    string? descripcion,
    string? iconoClave,
    bool activo
);
