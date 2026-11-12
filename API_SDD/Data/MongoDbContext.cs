// ---------------------------------------------------------------------------
// MongoDbContext – encapsula el acceso a MongoDB Atlas (BusTrackNoSQL).
// La cadena de conexión se lee desde configuración (MongoDb:ConnectionString).
// Nunca se almacena ni dispone antes de tiempo: un cliente singleton comparte el pool.
// ---------------------------------------------------------------------------
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ApiSDD.Data;

/// <summary>
/// Contexto MongoDB singleton utilizado por los servicios y checks de salud de la API.
/// </summary>
public sealed class MongoDbContext : IDisposable
{
    private readonly MongoClient? _client;
    private readonly IMongoDatabase? _database;

    public MongoDbContext(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var connectionString = configuration["MongoDb:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString)
            || connectionString.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("EXTERNA", StringComparison.OrdinalIgnoreCase))
        {
            IsConfigured = false;
            return;
        }

        try
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            settings.ConnectTimeout = TimeSpan.FromSeconds(5);
            settings.SocketTimeout = TimeSpan.FromSeconds(15);

            var dbName = MongoUrl.Create(connectionString).DatabaseName;
            if (string.IsNullOrWhiteSpace(dbName))
            {
                dbName = configuration["MongoDb:Database"] ?? "BusTrackNoSQL";
            }

            _client = new MongoClient(settings);
            _database = _client.GetDatabase(dbName);
            IsConfigured = true;
        }
        catch (Exception ex) when (ex is MongoConfigurationException or FormatException or ArgumentException)
        {
            IsConfigured = false;
            ConfigurationError = ex.Message;
        }
    }

    public bool IsConfigured { get; }

    public string? ConfigurationError { get; }

    public IMongoClient? Client => _client;

    public void Dispose() => _client?.Dispose();

    public IMongoDatabase Database =>
        _database ?? throw new InvalidOperationException("MongoDb:ConnectionString no está configurado.");

    public IMongoCollection<TDocument> GetCollection<TDocument>(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return Database.GetCollection<TDocument>(name);
    }

    public IMongoCollection<EmpresaDocument> Empresas =>
        Database.GetCollection<EmpresaDocument>("empresas");

    public IMongoCollection<MedioTransporteDocument> MediosTransporte =>
        Database.GetCollection<MedioTransporteDocument>("mediosTransporte");

    public IMongoCollection<RutaDocument> Rutas =>
        Database.GetCollection<RutaDocument>("rutas");

    public IMongoCollection<BsonDocument> RawCollection(string name) =>
        Database.GetCollection<BsonDocument>(name);
}

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
}
