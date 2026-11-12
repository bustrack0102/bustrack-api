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
        var usuarios = _mongo.RawCollection("usuarios");
        await usuarios.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("correo"),
                new CreateIndexOptions { Name = "ux_usuarios_correo", Unique = true }),
            cancellationToken: ct).ConfigureAwait(false);

        var paradas = _mongo.RawCollection("paradas");
        await paradas.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Geo2DSphere("ubicacion"),
                new CreateIndexOptions { Name = "geo_paradas_ubicacion" }),
            cancellationToken: ct).ConfigureAwait(false);

        var destinos = _mongo.RawCollection("destinos");
        await destinos.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Geo2DSphere("ubicacion"),
                new CreateIndexOptions { Name = "geo_destinos_ubicacion" }),
            cancellationToken: ct).ConfigureAwait(false);

        var candidatos = _mongo.RawCollection("candidatos_parada");
        await candidatos.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Geo2DSphere("ubicacion"),
                new CreateIndexOptions { Name = "geo_candidatos_parada" }),
            cancellationToken: ct).ConfigureAwait(false);

        var rutas = _mongo.RawCollection("rutas");
        await rutas.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("empresaId").Ascending("activa"),
                new CreateIndexOptions { Name = "ix_rutas_empresa_activa" }),
            cancellationToken: ct).ConfigureAwait(false);

        var empresas = _mongo.RawCollection("empresas");
        await empresas.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Text("nombre").Text("siglas").Text("descripcion"),
                new CreateIndexOptions { Name = "txt_empresas_busqueda" }),
            cancellationToken: ct).ConfigureAwait(false);

        var horarios = _mongo.RawCollection("horarios");
        await horarios.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("rutaId").Ascending("diaSemana"),
                new CreateIndexOptions { Name = "ix_horarios_ruta_dia" }),
            cancellationToken: ct).ConfigureAwait(false);

        var tarifas = _mongo.RawCollection("tarifas");
        await tarifas.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("rutaId").Ascending("vigente"),
                new CreateIndexOptions { Name = "ix_tarifas_ruta_vigente" }),
            cancellationToken: ct).ConfigureAwait(false);
    }
}
