using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ApiSDD.Data;

/// <summary>
/// Inicializa índices NoSQL (unique, compuestos y 2dsphere) en MongoDB Atlas
/// sin bloquear el arranque ni fallar en entornos de pruebas aislados.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "El inicializador en segundo plano debe registrar errores sin derribar el arranque del host.")]
internal sealed partial class MongoIndexInitializer : IHostedService
{
    private readonly MongoDbContext _mongo;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(MongoDbContext mongo, ILogger<MongoIndexInitializer> logger)
    {
        _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_mongo.IsConfigured)
        {
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            await EnsureCollectionIndexesAsync(cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogIndexInitWarning(_logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Omitiendo inicialización de índices MongoDB en arranque.")]
    private static partial void LogIndexInitWarning(ILogger logger, Exception ex);

    private async Task EnsureCollectionIndexesAsync(CancellationToken ct)
    {
        await CreateIndexSafeAsync(
            _mongo.RawCollection("paradas"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Geo2DSphere("ubicacion"),
                new CreateIndexOptions { Name = "geo_paradas_ubicacion" }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection("destinos"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Geo2DSphere("ubicacion"),
                new CreateIndexOptions { Name = "geo_destinos_ubicacion" }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection("candidatos_parada"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Geo2DSphere("ubicacion"),
                new CreateIndexOptions { Name = "geo_candidatos_parada" }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection("usuarios"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("correo"),
                new CreateIndexOptions<BsonDocument>
                {
                    Name = "ux_usuarios_correo",
                    Unique = true,
                    PartialFilterExpression = new BsonDocument("correo", new BsonDocument("$type", "string")),
                }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection("rutas"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("empresaId").Ascending("activa"),
                new CreateIndexOptions { Name = "ix_rutas_empresa_activa" }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection("empresas"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Text("nombre").Text("siglas").Text("descripcion"),
                new CreateIndexOptions { Name = "txt_empresas_busqueda" }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection("horarios"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("rutaId").Ascending("diaSemana"),
                new CreateIndexOptions { Name = "ix_horarios_ruta_dia" }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection("tarifas"),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("rutaId").Ascending("vigente"),
                new CreateIndexOptions { Name = "ix_tarifas_ruta_vigente" }),
            ct).ConfigureAwait(false);

        await CreateIndexSafeAsync(
            _mongo.RawCollection(ApiSDD.Data.NoSql.MongoDistributedCache.CollectionName),
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("expiraEnUtc"),
                new CreateIndexOptions { Name = "ttl_cache_expira", ExpireAfter = TimeSpan.Zero }),
            ct).ConfigureAwait(false);
    }

    private async Task CreateIndexSafeAsync(
        IMongoCollection<BsonDocument> collection,
        CreateIndexModel<BsonDocument> model,
        CancellationToken ct)
    {
        try
        {
            await collection.Indexes.CreateOneAsync(model, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException)
        {
            LogIndexInitWarning(_logger, ex);
        }
    }
}
