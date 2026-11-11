// ---------------------------------------------------------------------------
// MongoDbContext – encapsula el acceso a MongoDB Atlas.
// La cadena de conexión se lee desde la configuración (MongoDb:ConnectionString).
// Se mantiene fuera del repositorio mediante appsettings.Local.json o User Secrets.
// ---------------------------------------------------------------------------
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ApiSDD.Data;

/// <summary>
/// Contexto MongoDB utilizado por los servicios de la API.
/// Sólo expone las colecciones que necesita la capa de negocio.
/// </summary>
public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var connectionString = configuration["MongoDb:ConnectionString"]
                               ?? throw new InvalidOperationException("MongoDb:ConnectionString no está configurado");
        using var client = new MongoClient(connectionString);
        var dbName = MongoUrl.Create(connectionString).DatabaseName
                     ?? configuration["MongoDb:Database"]
                     ?? "BusTrackNoSQL";
        _database = client.GetDatabase(dbName);
    }

    public IMongoDatabase Database => _database;

    // ---------------------------------------------------------------------
    // Colecciones usadas por los endpoints de Empresas.
    // ---------------------------------------------------------------------
    public IMongoCollection<EmpresaDocument> Empresas =>
        _database.GetCollection<EmpresaDocument>("empresas");

    public IMongoCollection<MedioTransporteDocument> MediosTransporte =>
        _database.GetCollection<MedioTransporteDocument>("mediosTransporte");

    public IMongoCollection<RutaDocument> Rutas =>
        _database.GetCollection<RutaDocument>("rutas");
}

// ---------------------------------------------------------------------------
// POCOs (Plain Old CLR Objects) que representan los documentos MongoDB.
// Solo los campos necesarios para los endpoints de lectura están incluidos.
// ---------------------------------------------------------------------------
public sealed class EmpresaDocument
{
    [BsonId]
    public string Id { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string? Siglas { get; set; }
    public string? Descripcion { get; set; }
    public string MunicipioBase { get; set; } = default!;
    public double CalificacionPromedio { get; set; }
    public int TotalEvaluaciones { get; set; }
    public bool Activo { get; set; }
    public IEnumerable<string> MediosTransporte { get; set; } = Array.Empty<string>();
    public int TotalRutasActivas { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

public sealed class MedioTransporteDocument
{
    [BsonId]
    public string Id { get; set; } = default!;
    public string EmpresaId { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Categoria { get; set; } = default!;
    public bool Activo { get; set; }
}

public sealed class RutaDocument
{
    [BsonId]
    public string Id { get; set; } = default!;
    public string EmpresaId { get; set; } = default!;
    public string MedioTransporteId { get; set; } = default!;
    // Campos adicionales pueden añadirse en el futuro.
}
