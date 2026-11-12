using MongoDB.Bson.Serialization.Attributes;

namespace ApiSDD.Data.NoSql;

/// <summary>Punto GeoJSON compatible con índice 2dsphere de MongoDB ([longitud, latitud]).</summary>
public sealed class GeoJsonPointDoc
{
    [BsonElement("type")]
    public string Type { get; set; } = "Point";

    [BsonElement("coordinates")]
    public IReadOnlyList<double> Coordinates { get; set; } = [0.0, 0.0];

    public static GeoJsonPointDoc FromLatLon(double lat, double lon) =>
        new() { Type = "Point", Coordinates = [lon, lat] };

    [BsonIgnore]
    public double Latitud => Coordinates.Count >= 2 ? Coordinates[1] : 0.0;

    [BsonIgnore]
    public double Longitud => Coordinates.Count >= 1 ? Coordinates[0] : 0.0;
}

public sealed class RolDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }
}

public sealed class UsuarioDoc
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("nombreCompleto")]
    public string NombreCompleto { get; set; } = default!;

    [BsonElement("correo")]
    public string Correo { get; set; } = default!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = default!;

    [BsonElement("rolNombre")]
    public string RolNombre { get; set; } = "Usuario";

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;

    [BsonElement("consentimientoSensores")]
    public bool ConsentimientoSensores { get; set; }

    [BsonElement("fechaAvisoPrivacidad")]
    public DateTime? FechaAvisoPrivacidad { get; set; }

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonElement("fechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }

    [BsonElement("ultimoAcceso")]
    public DateTime? UltimoAcceso { get; set; }
}

public sealed class RefreshTokenDoc
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("usuarioId")]
    public Guid UsuarioId { get; set; }

    [BsonElement("tokenHash")]
    public string TokenHash { get; set; } = default!;

    [BsonElement("dispositivoInfo")]
    public string? DispositivoInfo { get; set; }

    [BsonElement("fechaExpiracion")]
    public DateTime FechaExpiracion { get; set; }

    [BsonElement("revocado")]
    public bool Revocado { get; set; }

    [BsonElement("fechaRevocacion")]
    public DateTime? FechaRevocacion { get; set; }

    [BsonElement("reemplazadoPorTokenId")]
    public Guid? ReemplazadoPorTokenId { get; set; }
}

public sealed class TokenRecuperacionDoc
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("usuarioId")]
    public Guid UsuarioId { get; set; }

    [BsonElement("tokenHash")]
    public string TokenHash { get; set; } = default!;

    [BsonElement("fechaExpiracion")]
    public DateTime FechaExpiracion { get; set; }

    [BsonElement("usado")]
    public bool Usado { get; set; }

    [BsonElement("fechaUso")]
    public DateTime? FechaUso { get; set; }

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
}

public sealed class BloqueoLoginDoc
{
    [BsonId]
    public string NombreUsuario { get; set; } = default!;

    [BsonElement("intentos")]
    public int Intentos { get; set; }

    [BsonElement("bloqueoHasta")]
    public DateTime? BloqueoHasta { get; set; }
}

public sealed class MedioTransporteDoc
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("clave")]
    public string Clave { get; set; } = default!;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }

    [BsonElement("iconoClave")]
    public string? IconoClave { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;

    [BsonElement("fechaActualizacion")]
    public DateTime FechaActualizacion { get; set; }
}

public sealed class EmpresaDoc
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("siglas")]
    public string? Siglas { get; set; }

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }

    [BsonElement("municipioBase")]
    public string MunicipioBase { get; set; } = default!;

    [BsonElement("telefonoContacto")]
    public string? TelefonoContacto { get; set; }

    [BsonElement("sitioWeb")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-returning properties should not be strings", Justification = "Contrato JSON/BSON estable.")]
    public string? SitioWeb { get; set; }

    [BsonElement("logoUrl")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-returning properties should not be strings", Justification = "Contrato JSON/BSON estable.")]
    public string? LogoUrl { get; set; }

    [BsonElement("calificacionPromedio")]
    public double CalificacionPromedio { get; set; }

    [BsonElement("totalEvaluaciones")]
    public int TotalEvaluaciones { get; set; }

    [BsonElement("mediosTransporte")]
    public IReadOnlyList<string> MediosTransporte { get; set; } = [];

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;

    [BsonElement("fuenteDatoId")]
    public Guid? FuenteDatoId { get; set; }

    [BsonElement("fechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }
}

public sealed class ParadaDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("codigo")]
    public string? Codigo { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("municipio")]
    public string Municipio { get; set; } = default!;

    [BsonElement("tipo")]
    public string Tipo { get; set; } = "Intermedia";

    [BsonElement("esTerminal")]
    public bool EsTerminal { get; set; }

    [BsonElement("referencias")]
    public string? Referencias { get; set; }

    [BsonElement("ubicacion")]
    public GeoJsonPointDoc Ubicacion { get; set; } = new();

    [BsonElement("techada")]
    public bool Techada { get; set; } = true;

    [BsonElement("rutasDisponibles")]
    public int RutasDisponibles { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;

    [BsonElement("fechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }
}

public sealed class ParadaEnRutaDoc
{
    [BsonElement("paradaId")]
    public string ParadaId { get; set; } = default!;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("referenciaVisual")]
    public string? ReferenciaVisual { get; set; }

    [BsonElement("municipio")]
    public string? Municipio { get; set; }

    [BsonElement("latitud")]
    public double Latitud { get; set; }

    [BsonElement("longitud")]
    public double Longitud { get; set; }

    [BsonElement("ordenSecuencia")]
    public int OrdenSecuencia { get; set; }

    [BsonElement("tiempoDesdeOrigenMin")]
    public int TiempoDesdeOrigenMin { get; set; }

    [BsonElement("distanciaDesdeOrigenKm")]
    public double DistanciaDesdeOrigenKm { get; set; }

    [BsonElement("permiteAscenso")]
    public bool PermiteAscenso { get; set; } = true;

    [BsonElement("permiteDescenso")]
    public bool PermiteDescenso { get; set; } = true;
}

public sealed class RutaDoc
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("codigo")]
    public string Codigo { get; set; } = default!;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }

    [BsonElement("empresaId")]
    public Guid EmpresaId { get; set; }

    [BsonElement("empresaNombre")]
    public string EmpresaNombre { get; set; } = default!;

    [BsonElement("medioTransporteId")]
    public int MedioTransporteId { get; set; }

    [BsonElement("medioTransporteNombre")]
    public string MedioTransporteNombre { get; set; } = default!;

    [BsonElement("paradaOrigenId")]
    public string? ParadaOrigenId { get; set; }

    [BsonElement("paradaDestinoId")]
    public string? ParadaDestinoId { get; set; }

    [BsonElement("paradaOrigen")]
    public string ParadaOrigen { get; set; } = default!;

    [BsonElement("paradaDestino")]
    public string ParadaDestino { get; set; } = default!;

    [BsonElement("municipio")]
    public string Municipio { get; set; } = default!;

    [BsonElement("tarifaBaseMxn")]
    public decimal TarifaBaseMxn { get; set; }

    [BsonElement("duracionEstimadaMin")]
    public int DuracionEstimadaMin { get; set; }

    [BsonElement("distanciaKm")]
    public double DistanciaKm { get; set; }

    [BsonElement("primerHorario")]
    public string PrimerHorario { get; set; } = default!;

    [BsonElement("ultimoHorario")]
    public string UltimoHorario { get; set; } = default!;

    [BsonElement("frecuenciaMinutos")]
    public int FrecuenciaMinutos { get; set; }

    [BsonElement("diasOperacion")]
    public string DiasOperacion { get; set; } = default!;

    [BsonElement("calificacionPromedio")]
    public double CalificacionPromedio { get; set; }

    [BsonElement("totalCalificaciones")]
    public int TotalCalificaciones { get; set; }

    [BsonElement("activa")]
    public bool Activa { get; set; } = true;

    [BsonElement("trazado")]
    public IReadOnlyList<double[]>? Trazado { get; set; }

    [BsonElement("fuenteDatoId")]
    public Guid? FuenteDatoId { get; set; }

    [BsonElement("fechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }

    [BsonElement("paradas")]
    public IReadOnlyList<ParadaEnRutaDoc> Paradas { get; set; } = [];
}

public sealed class HorarioDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("rutaId")]
    public Guid RutaId { get; set; }

    [BsonElement("diaSemana")]
    public string DiaSemana { get; set; } = default!;

    [BsonElement("horaInicio")]
    public string HoraInicio { get; set; } = default!;

    [BsonElement("horaFin")]
    public string HoraFin { get; set; } = default!;

    [BsonElement("frecuenciaMin")]
    public int FrecuenciaMin { get; set; }

    [BsonElement("salidasFijas")]
    public IReadOnlyList<string> SalidasFijas { get; set; } = [];

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;
}

public sealed class TarifaDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("rutaId")]
    public Guid RutaId { get; set; }

    [BsonElement("tipoTarifa")]
    public string TipoTarifa { get; set; } = "General";

    [BsonElement("montoMxn")]
    public decimal MontoMxn { get; set; }

    [BsonElement("descuentoEstudiantePct")]
    public int DescuentoEstudiantePct { get; set; }

    [BsonElement("moneda")]
    public string Moneda { get; set; } = "MXN";

    [BsonElement("vigente")]
    public bool Vigente { get; set; } = true;

    [BsonElement("notas")]
    public string? Notas { get; set; }
}

public sealed class DestinoDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("categoria")]
    public string Categoria { get; set; } = default!;

    [BsonElement("municipio")]
    public string Municipio { get; set; } = default!;

    [BsonElement("descripcion")]
    public string Descripcion { get; set; } = default!;

    [BsonElement("paradaCercanaId")]
    public string ParadaCercanaId { get; set; } = default!;

    [BsonElement("rutasIds")]
    public IReadOnlyList<Guid> RutasIds { get; set; } = [];

    [BsonElement("ubicacion")]
    public GeoJsonPointDoc Ubicacion { get; set; } = new();

    [BsonElement("calificacionPromedio")]
    public double CalificacionPromedio { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;
}

public sealed class CalificacionDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("usuarioId")]
    public Guid UsuarioId { get; set; }

    [BsonElement("usuarioNombre")]
    public string UsuarioNombre { get; set; } = default!;

    [BsonElement("rutaId")]
    public Guid? RutaId { get; set; }

    [BsonElement("empresaId")]
    public Guid? EmpresaId { get; set; }

    [BsonElement("destinoId")]
    public string? DestinoId { get; set; }

    [BsonElement("puntuacion")]
    public int Puntuacion { get; set; }

    [BsonElement("comentario")]
    public string? Comentario { get; set; }

    [BsonElement("imagenUrl")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-returning properties should not be strings", Justification = "Contrato JSON/BSON estable.")]
    public string? ImagenUrl { get; set; }

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonElement("fechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }
}

public sealed class ViajeGuardadoDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("usuarioId")]
    public Guid UsuarioId { get; set; }

    [BsonElement("titulo")]
    public string Titulo { get; set; } = default!;

    [BsonElement("rutaId")]
    public Guid RutaId { get; set; }

    [BsonElement("paradaOrigenId")]
    public string? ParadaOrigenId { get; set; }

    [BsonElement("paradaDestinoId")]
    public string? ParadaDestinoId { get; set; }

    [BsonElement("notas")]
    public string? Notas { get; set; }

    [BsonElement("favorito")]
    public bool Favorito { get; set; }

    [BsonElement("fechaActualizacion")]
    public DateTime FechaActualizacion { get; set; }
}

public sealed class ConsentimientoSensorDoc
{
    [BsonId]
    public Guid UsuarioId { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; }

    [BsonElement("versionAviso")]
    public string VersionAviso { get; set; } = "1.0";

    [BsonElement("fechaActualizacion")]
    public DateTime FechaActualizacion { get; set; }
}

public sealed class LoteSensorDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("usuarioHash")]
    public string UsuarioHash { get; set; } = default!;

    [BsonElement("rutaId")]
    public Guid RutaId { get; set; }

    [BsonElement("totalMuestras")]
    public int TotalMuestras { get; set; }

    [BsonElement("clasificacionPrincipal")]
    public string ClasificacionPrincipal { get; set; } = default!;

    [BsonElement("confianzaGlobal")]
    public double ConfianzaGlobal { get; set; }

    [BsonElement("creadoEnUtc")]
    public DateTime CreadoEnUtc { get; set; }
}

public sealed class CandidatoParadaDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("rutaId")]
    public Guid RutaId { get; set; }

    [BsonElement("loteId")]
    public string LoteId { get; set; } = default!;

    [BsonElement("usuarioId")]
    public Guid UsuarioId { get; set; }

    [BsonElement("nombreSugerido")]
    public string NombreSugerido { get; set; } = default!;

    [BsonElement("municipio")]
    public string Municipio { get; set; } = default!;

    [BsonElement("ubicacion")]
    public GeoJsonPointDoc Ubicacion { get; set; } = new();

    [BsonElement("confianza")]
    public double Confianza { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = "Pendiente";

    [BsonElement("creadoEnUtc")]
    public DateTime CreadoEnUtc { get; set; }
}

public sealed class SolicitudRecomendacionDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("usuarioId")]
    public Guid? UsuarioId { get; set; }

    [BsonElement("origen")]
    public string Origen { get; set; } = default!;

    [BsonElement("destino")]
    public string Destino { get; set; } = default!;

    [BsonElement("prioridad")]
    public string Prioridad { get; set; } = default!;

    [BsonElement("rutaRecomendadaId")]
    public Guid? RutaRecomendadaId { get; set; }

    [BsonElement("explicacion")]
    public string Explicacion { get; set; } = default!;

    [BsonElement("modeloUsado")]
    public string ModeloUsado { get; set; } = default!;

    [BsonElement("utilidadPuntuacion")]
    public int? UtilidadPuntuacion { get; set; }

    [BsonElement("comentarioFeedback")]
    public string? ComentarioFeedback { get; set; }

    [BsonElement("creadoEnUtc")]
    public DateTime CreadoEnUtc { get; set; }
}

public sealed class FuenteDatoDoc
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = default!;

    [BsonElement("tipoFuente")]
    public string TipoFuente { get; set; } = default!;

    [BsonElement("responsable")]
    public string Responsable { get; set; } = default!;

    [BsonElement("referenciaUrl")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-returning properties should not be strings", Justification = "Contrato JSON/BSON estable.")]
    public string? ReferenciaUrl { get; set; }

    [BsonElement("activa")]
    public bool Activa { get; set; } = true;

    [BsonElement("actualizadaEnUtc")]
    public DateTime ActualizadaEnUtc { get; set; }
}

public sealed class ImportacionDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("fuenteDatoId")]
    public Guid FuenteDatoId { get; set; }

    [BsonElement("usuarioAdminId")]
    public Guid UsuarioAdminId { get; set; }

    [BsonElement("tipoEntidad")]
    public string TipoEntidad { get; set; } = default!;

    [BsonElement("registrosProcesados")]
    public int RegistrosProcesados { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = "Completada";

    [BsonElement("ejecutadoEnUtc")]
    public DateTime EjecutadoEnUtc { get; set; }
}

public sealed class DispositivoFcmDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("usuarioId")]
    public Guid UsuarioId { get; set; }

    [BsonElement("tokenFcm")]
    public string TokenFcm { get; set; } = default!;

    [BsonElement("plataforma")]
    public string Plataforma { get; set; } = "android";

    [BsonElement("actualizadoEnUtc")]
    public DateTime ActualizadoEnUtc { get; set; }
}
