using ApiSDD.Data;
using ApiSDD.Data.NoSql;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace UnitTest.Data;

/// <summary>
/// Valida que todos los documentos NoSQL de BusTrack serialicen y deserialicen con MongoDB.Driver 3.x
/// sin errores de representación Guid ni pérdida de datos (incluyendo campos nulos y GeoJSON).
/// </summary>
public sealed class BsonSerializationTests
{
    [Fact]
    public void EnsureBsonConventionsRegisteredEsIdempotenteYSerializaTodosLosDocumentos()
    {
        MongoDbContext.EnsureBsonConventionsRegistered();
        MongoDbContext.EnsureBsonConventionsRegistered();

        var userId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var now = new DateTime(2026, 11, 12, 5, 0, 0, DateTimeKind.Utc);

        RoundTrip(new RolDoc { Id = "Usuario", Nombre = "Usuario", Descripcion = "Pasajero" })
            .Id.Should().Be("Usuario");

        RoundTrip(new UsuarioDoc
        {
            Id = userId,
            NombreCompleto = "Ana Prueba",
            Correo = "ana@bustrack.mx",
            PasswordHash = "hash",
            RolNombre = "Usuario",
            Activo = true,
            ConsentimientoSensores = true,
            FechaAvisoPrivacidad = now,
            FechaCreacion = now,
            FechaActualizacion = now,
            UltimoAcceso = now,
        }).Id.Should().Be(userId);

        RoundTrip(new RefreshTokenDoc
        {
            Id = Guid.NewGuid(),
            UsuarioId = userId,
            TokenHash = "tok",
            DispositivoInfo = "pixel",
            FechaExpiracion = now.AddDays(7),
            Revocado = false,
            ReemplazadoPorTokenId = null,
        }).UsuarioId.Should().Be(userId);

        RoundTrip(new TokenRecuperacionDoc
        {
            Id = Guid.NewGuid(),
            UsuarioId = userId,
            TokenHash = "rec",
            FechaExpiracion = now.AddHours(1),
            Usado = false,
            FechaCreacion = now,
        }).UsuarioId.Should().Be(userId);

        RoundTrip(new BloqueoLoginDoc
        {
            NombreUsuario = "ana@bustrack.mx",
            Intentos = 2,
            BloqueoHasta = now.AddMinutes(15),
        }).Intentos.Should().Be(2);

        foreach (var medio in BusTrackSeedData.MediosTransporte())
        {
            RoundTrip(medio).Id.Should().Be(medio.Id);
        }

        foreach (var fuente in BusTrackSeedData.FuentesDatos())
        {
            RoundTrip(fuente).Id.Should().Be(fuente.Id);
        }

        foreach (var empresa in BusTrackSeedData.Empresas())
        {
            var copy = RoundTrip(empresa);
            copy.Id.Should().Be(empresa.Id);
            copy.FuenteDatoId.Should().Be(empresa.FuenteDatoId);
        }

        foreach (var parada in BusTrackSeedData.Paradas())
        {
            var copy = RoundTrip(parada);
            copy.Id.Should().Be(parada.Id);
            copy.Ubicacion.Coordinates.Should().HaveCount(2);
        }

        foreach (var ruta in BusTrackSeedData.Rutas())
        {
            var copy = RoundTrip(ruta);
            copy.Id.Should().Be(ruta.Id);
            copy.EmpresaId.Should().Be(ruta.EmpresaId);
            copy.TarifaBaseMxn.Should().Be(ruta.TarifaBaseMxn);
        }

        foreach (var horario in BusTrackSeedData.Horarios())
        {
            RoundTrip(horario).RutaId.Should().Be(horario.RutaId);
        }

        foreach (var tarifa in BusTrackSeedData.Tarifas())
        {
            var copy = RoundTrip(tarifa);
            copy.RutaId.Should().Be(tarifa.RutaId);
            copy.MontoMxn.Should().Be(tarifa.MontoMxn);
        }

        foreach (var destino in BusTrackSeedData.Destinos())
        {
            var copy = RoundTrip(destino);
            copy.Id.Should().Be(destino.Id);
            copy.RutasIds.Should().Equal(destino.RutasIds);
        }

        RoundTrip(new CalificacionDoc
        {
            Id = "cal-1",
            UsuarioId = userId,
            UsuarioNombre = "Ana",
            RutaId = BusTrackSeedData.Ruta1Id,
            EmpresaId = null,
            DestinoId = "DES-01",
            Puntuacion = 5,
            Comentario = "Puntual",
            ImagenUrl = "https://example.invalid/foto.jpg",
            FechaCreacion = now,
        }).DestinoId.Should().Be("DES-01");

        RoundTrip(new ViajeGuardadoDoc
        {
            Id = "vg-1",
            UsuarioId = userId,
            Titulo = "A UTTT",
            RutaId = BusTrackSeedData.Ruta1Id,
            ParadaOrigenId = "PAR-01",
            ParadaDestinoId = "PAR-04",
            Favorito = true,
            FechaActualizacion = now,
        }).Favorito.Should().BeTrue();

        RoundTrip(new ConsentimientoSensorDoc
        {
            UsuarioId = userId,
            Activo = true,
            VersionAviso = "1.1",
            FechaActualizacion = now,
        }).Activo.Should().BeTrue();

        RoundTrip(new LoteSensorDoc
        {
            Id = "lote-1",
            UsuarioHash = "hash123",
            RutaId = BusTrackSeedData.Ruta1Id,
            TotalMuestras = 10,
            ClasificacionPrincipal = "EN_TRAMO",
            ConfianzaGlobal = 0.91,
            CreadoEnUtc = now,
        }).TotalMuestras.Should().Be(10);

        RoundTrip(new CandidatoParadaDoc
        {
            Id = "cand-1",
            RutaId = BusTrackSeedData.Ruta1Id,
            LoteId = "lote-1",
            UsuarioId = userId,
            NombreSugerido = "UTTT Acceso",
            Municipio = "Tula de Allende",
            Ubicacion = GeoJsonPointDoc.FromLatLon(20.0512, -99.3421),
            Confianza = 0.9,
            Estado = "Pendiente",
            CreadoEnUtc = now,
        }).Ubicacion.Latitud.Should().BeApproximately(20.0512, 1e-6);

        RoundTrip(new SolicitudRecomendacionDoc
        {
            Id = "rec-1",
            UsuarioId = null,
            Origen = "Centro",
            Destino = "UTTT",
            Prioridad = "rapidez",
            RutaRecomendadaId = BusTrackSeedData.Ruta1Id,
            Explicacion = "Ruta directa.",
            ModeloUsado = "bustrack-genai",
            UtilidadPuntuacion = 5,
            CreadoEnUtc = now,
        }).RutaRecomendadaId.Should().Be(BusTrackSeedData.Ruta1Id);

        RoundTrip(new ImportacionDoc
        {
            Id = "imp-1",
            FuenteDatoId = BusTrackSeedData.FuenteOficialId,
            UsuarioAdminId = userId,
            TipoEntidad = "rutas",
            RegistrosProcesados = 4,
            Estado = "Completada",
            EjecutadoEnUtc = now,
        }).FuenteDatoId.Should().Be(BusTrackSeedData.FuenteOficialId);

        RoundTrip(new DispositivoFcmDoc
        {
            Id = "dev-1",
            UsuarioId = userId,
            TokenFcm = "fcm-tok",
            Plataforma = "android",
            ActualizadoEnUtc = now,
        }).UsuarioId.Should().Be(userId);
    }

    [Fact]
    public void DeserializarIgnoraCamposExtraEnMongoDb()
    {
        MongoDbContext.EnsureBsonConventionsRegistered();
        var doc = new BsonDocument
        {
            ["_id"] = new BsonBinaryData(BusTrackSeedData.EmpresaAvmId, GuidRepresentation.Standard),
            ["nombre"] = "AVM",
            ["municipioBase"] = "Tula de Allende",
            ["campoLegadoExtra"] = "valor-ignorado",
        };

        var deserialized = BsonSerializer.Deserialize<EmpresaDoc>(doc);
        deserialized.Id.Should().Be(BusTrackSeedData.EmpresaAvmId);
        deserialized.Nombre.Should().Be("AVM");
    }

    [Fact]
    public void ConstructorMongoDbContextConConfiguracionVaciaNoFalla()
    {
        var config = new ConfigurationBuilder().Build();
        using var ctx = new MongoDbContext(config);
        ctx.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void MongoDistributedCacheSerializaDocumentoYCalculaTtl()
    {
        MongoDbContext.EnsureBsonConventionsRegistered();
        var now = new DateTime(2026, 11, 12, 6, 0, 0, DateTimeKind.Utc);
        var doc = MongoDistributedCache.BuildDocument(
            "blacklist:jti-123",
            "1794500000"u8.ToArray(),
            new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
            },
            now);

        var copy = RoundTrip(doc);
        copy.Id.Should().Be("blacklist:jti-123");
        copy.ExpiraEnUtc.Should().Be(now.AddMinutes(15));
        Convert.FromBase64String(copy.ValorBase64).Should().Equal("1794500000"u8.ToArray());
    }

    private static T RoundTrip<T>(T value)
    {
        var bson = value.ToBson();
        return BsonSerializer.Deserialize<T>(bson);
    }
}
