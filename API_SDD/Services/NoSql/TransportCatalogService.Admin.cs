using System.Globalization;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Operational;
using ApiSDD.Dtos.Transport;

namespace ApiSDD.Services.NoSql;

internal sealed partial class TransportCatalogService
{
    public async Task<ParadaDetallePublicoDto> CreateParadaAsync(ParadaUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCoordinates(request.Latitud, request.Longitud);
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Municipio))
        {
            throw new RequestValidationException("Nombre y municipio de la parada son obligatorios.");
        }

        var id = string.IsNullOrWhiteSpace(request.Id)
            ? $"PAR-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
            : request.Id.Trim();

        if (await _store.GetParadaByIdAsync(id, ct).ConfigureAwait(false) is not null)
        {
            throw new ConflictException($"Ya existe una parada con identificador '{id}'.");
        }

        var doc = new ParadaDoc
        {
            Id = id,
            Nombre = request.Nombre.Trim(),
            Municipio = request.Municipio.Trim(),
            Referencias = request.Referencias?.Trim(),
            Ubicacion = GeoJsonPointDoc.FromLatLon(request.Latitud, request.Longitud),
            Techada = request.Techada,
            RutasDisponibles = 0,
            Activo = request.Activo,
        };

        await _store.UpsertParadaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapParada(doc);
    }

    public async Task<ParadaDetallePublicoDto> UpdateParadaAsync(string id, ParadaUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCoordinates(request.Latitud, request.Longitud);
        var doc = await _store.GetParadaByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Parada no encontrada.");

        doc.Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? doc.Nombre : request.Nombre.Trim();
        doc.Municipio = string.IsNullOrWhiteSpace(request.Municipio) ? doc.Municipio : request.Municipio.Trim();
        doc.Referencias = request.Referencias?.Trim();
        doc.Ubicacion = GeoJsonPointDoc.FromLatLon(request.Latitud, request.Longitud);
        doc.Techada = request.Techada;
        doc.Activo = request.Activo;

        await _store.UpsertParadaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapParada(doc);
    }

    public async Task DeactivateParadaAsync(string id, CancellationToken ct)
    {
        var doc = await _store.GetParadaByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Parada no encontrada.");
        doc.Activo = false;
        await _store.UpsertParadaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
    }

    public async Task<RutaListItemDto> CreateRutaAsync(RutaUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRutaRequest(request);

        var existing = (await _store.ListRutasAsync(ct).ConfigureAwait(false))
            .Any(r => string.Equals(r.Codigo, request.Codigo.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing)
        {
            throw new ConflictException($"El código de ruta '{request.Codigo}' ya existe.");
        }

        var empresa = await _store.GetEmpresaByIdAsync(request.EmpresaId, ct).ConfigureAwait(false)
                      ?? throw new NotFoundException("Empresa no encontrada.");
        var medio = await _store.GetMedioByIdAsync(request.MedioTransporteId, ct).ConfigureAwait(false)
                    ?? throw new NotFoundException("Medio de transporte no encontrado.");

        var paradas = await ResolveParadasOrdenadasAsync(request.ParadasIds, ct).ConfigureAwait(false);
        var doc = new RutaDoc
        {
            Id = Guid.NewGuid(),
            Codigo = request.Codigo.Trim(),
            Nombre = request.Nombre.Trim(),
            EmpresaId = empresa.Id,
            EmpresaNombre = empresa.Nombre,
            MedioTransporteId = medio.Id,
            MedioTransporteNombre = medio.Nombre,
            ParadaOrigen = request.ParadaOrigen.Trim(),
            ParadaDestino = request.ParadaDestino.Trim(),
            Municipio = empresa.MunicipioBase,
            TarifaBaseMxn = request.TarifaBaseMxn,
            DuracionEstimadaMin = Math.Max(5, request.DuracionEstimadaMin),
            DistanciaKm = Math.Max(1.0, paradas.Count * 1.8),
            PrimerHorario = request.PrimerHorario.Trim(),
            UltimoHorario = request.UltimoHorario.Trim(),
            FrecuenciaMinutos = Math.Max(1, request.FrecuenciaMinutos),
            DiasOperacion = request.DiasOperacion.Trim(),
            CalificacionPromedio = 4.5,
            TotalCalificaciones = 0,
            Activa = request.Activa,
            Paradas = paradas,
        };

        await _store.UpsertRutaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapRuta(doc);
    }

    public async Task<RutaListItemDto> UpdateRutaAsync(Guid id, RutaUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRutaRequest(request);

        var doc = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Ruta no encontrada.");
        var duplicate = (await _store.ListRutasAsync(ct).ConfigureAwait(false))
            .Any(r => r.Id != id && string.Equals(r.Codigo, request.Codigo.Trim(), StringComparison.OrdinalIgnoreCase));
        if (duplicate)
        {
            throw new ConflictException($"El código de ruta '{request.Codigo}' pertenece a otra ruta.");
        }

        var empresa = await _store.GetEmpresaByIdAsync(request.EmpresaId, ct).ConfigureAwait(false)
                      ?? throw new NotFoundException("Empresa no encontrada.");
        var medio = await _store.GetMedioByIdAsync(request.MedioTransporteId, ct).ConfigureAwait(false)
                    ?? throw new NotFoundException("Medio de transporte no encontrado.");

        doc.Codigo = request.Codigo.Trim();
        doc.Nombre = request.Nombre.Trim();
        doc.EmpresaId = empresa.Id;
        doc.EmpresaNombre = empresa.Nombre;
        doc.MedioTransporteId = medio.Id;
        doc.MedioTransporteNombre = medio.Nombre;
        doc.ParadaOrigen = request.ParadaOrigen.Trim();
        doc.ParadaDestino = request.ParadaDestino.Trim();
        doc.TarifaBaseMxn = request.TarifaBaseMxn;
        doc.PrimerHorario = request.PrimerHorario.Trim();
        doc.UltimoHorario = request.UltimoHorario.Trim();
        doc.FrecuenciaMinutos = Math.Max(1, request.FrecuenciaMinutos);
        doc.DiasOperacion = request.DiasOperacion.Trim();
        doc.DuracionEstimadaMin = Math.Max(5, request.DuracionEstimadaMin);
        doc.Activa = request.Activa;
        if (request.ParadasIds is not null)
        {
            doc.Paradas = await ResolveParadasOrdenadasAsync(request.ParadasIds, ct).ConfigureAwait(false);
        }

        await _store.UpsertRutaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapRuta(doc);
    }

    public async Task DeactivateRutaAsync(Guid id, CancellationToken ct)
    {
        var doc = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Ruta no encontrada.");
        doc.Activa = false;
        await _store.UpsertRutaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
    }

    public async Task<HorarioItemDto> CreateHorarioAsync(HorarioUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateHorarioRequest(request);
        if (await _store.GetRutaByIdAsync(request.RutaId, ct).ConfigureAwait(false) is null)
        {
            throw new NotFoundException("Ruta no encontrada.");
        }

        var doc = new HorarioDoc
        {
            Id = $"HOR-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            RutaId = request.RutaId,
            DiaSemana = request.DiaSemana.Trim(),
            HoraInicio = request.HoraInicio.Trim(),
            HoraFin = request.HoraFin.Trim(),
            FrecuenciaMin = request.FrecuenciaMin,
            SalidasFijas = request.SalidasFijas ?? [request.HoraInicio.Trim(), request.HoraFin.Trim()],
            Activo = request.Activo,
        };

        await _store.UpsertHorarioAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapHorario(doc);
    }

    public async Task<HorarioItemDto> UpdateHorarioAsync(string id, HorarioUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateHorarioRequest(request);
        var doc = await _store.GetHorarioByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Horario no encontrado.");

        doc.DiaSemana = request.DiaSemana.Trim();
        doc.HoraInicio = request.HoraInicio.Trim();
        doc.HoraFin = request.HoraFin.Trim();
        doc.FrecuenciaMin = request.FrecuenciaMin;
        doc.SalidasFijas = request.SalidasFijas ?? doc.SalidasFijas;
        doc.Activo = request.Activo;

        await _store.UpsertHorarioAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapHorario(doc);
    }

    public async Task DeactivateHorarioAsync(string id, CancellationToken ct)
    {
        var doc = await _store.GetHorarioByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Horario no encontrado.");
        doc.Activo = false;
        await _store.UpsertHorarioAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
    }

    public async Task<TarifaItemDto> CreateTarifaAsync(TarifaUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTarifaRequest(request);
        if (await _store.GetRutaByIdAsync(request.RutaId, ct).ConfigureAwait(false) is null)
        {
            throw new NotFoundException("Ruta no encontrada.");
        }

        var doc = new TarifaDoc
        {
            Id = $"TAR-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            RutaId = request.RutaId,
            TipoTarifa = request.TipoTarifa.Trim(),
            MontoMxn = request.MontoMxn,
            DescuentoEstudiantePct = Math.Clamp(request.DescuentoEstudiantePct, 0, 100),
            Moneda = string.IsNullOrWhiteSpace(request.Moneda) ? "MXN" : request.Moneda.Trim().ToUpperInvariant(),
            Vigente = request.Vigente,
            Notas = request.Notas?.Trim(),
        };

        await _store.UpsertTarifaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapTarifa(doc);
    }

    public async Task<TarifaItemDto> UpdateTarifaAsync(string id, TarifaUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTarifaRequest(request);
        var doc = await _store.GetTarifaByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Tarifa no encontrada.");

        doc.TipoTarifa = request.TipoTarifa.Trim();
        doc.MontoMxn = request.MontoMxn;
        doc.DescuentoEstudiantePct = Math.Clamp(request.DescuentoEstudiantePct, 0, 100);
        doc.Moneda = string.IsNullOrWhiteSpace(request.Moneda) ? doc.Moneda : request.Moneda.Trim().ToUpperInvariant();
        doc.Vigente = request.Vigente;
        doc.Notas = request.Notas?.Trim();

        await _store.UpsertTarifaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapTarifa(doc);
    }

    public async Task DeactivateTarifaAsync(string id, CancellationToken ct)
    {
        var doc = await _store.GetTarifaByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Tarifa no encontrada.");
        doc.Vigente = false;
        await _store.UpsertTarifaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
    }

    public async Task<DestinoTuristicoDto> CreateDestinoAsync(DestinoUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCoordinates(request.Latitud, request.Longitud);
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Categoria))
        {
            throw new RequestValidationException("Nombre y categoría del destino son obligatorios.");
        }

        var id = string.IsNullOrWhiteSpace(request.Id)
            ? $"DES-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
            : request.Id.Trim();
        if (await _store.GetDestinoByIdAsync(id, ct).ConfigureAwait(false) is not null)
        {
            throw new ConflictException($"Ya existe un destino con id '{id}'.");
        }

        var doc = new DestinoDoc
        {
            Id = id,
            Nombre = request.Nombre.Trim(),
            Categoria = request.Categoria.Trim(),
            Municipio = request.Municipio.Trim(),
            Descripcion = request.Descripcion.Trim(),
            ParadaCercanaId = request.ParadaCercanaId.Trim(),
            Ubicacion = GeoJsonPointDoc.FromLatLon(request.Latitud, request.Longitud),
            CalificacionPromedio = 4.6,
            RutasIds = request.RutasIds ?? [],
            Activo = request.Activo,
        };

        await _store.UpsertDestinoAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapDestino(doc);
    }

    public async Task<DestinoTuristicoDto> UpdateDestinoAsync(string id, DestinoUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCoordinates(request.Latitud, request.Longitud);
        var doc = await _store.GetDestinoByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Destino turístico no encontrado.");

        doc.Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? doc.Nombre : request.Nombre.Trim();
        doc.Categoria = string.IsNullOrWhiteSpace(request.Categoria) ? doc.Categoria : request.Categoria.Trim();
        doc.Municipio = string.IsNullOrWhiteSpace(request.Municipio) ? doc.Municipio : request.Municipio.Trim();
        doc.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? doc.Descripcion : request.Descripcion.Trim();
        doc.ParadaCercanaId = request.ParadaCercanaId.Trim();
        doc.Ubicacion = GeoJsonPointDoc.FromLatLon(request.Latitud, request.Longitud);
        if (request.RutasIds is not null)
        {
            doc.RutasIds = request.RutasIds;
        }

        doc.Activo = request.Activo;
        await _store.UpsertDestinoAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapDestino(doc);
    }

    public async Task DeactivateDestinoAsync(string id, CancellationToken ct)
    {
        var doc = await _store.GetDestinoByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Destino turístico no encontrado.");
        doc.Activo = false;
        await _store.UpsertDestinoAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<ParadaEnRutaDoc>> ResolveParadasOrdenadasAsync(
        IReadOnlyList<string>? ids,
        CancellationToken ct)
    {
        if (ids is null || ids.Count == 0)
        {
            return [];
        }

        var list = new List<ParadaEnRutaDoc>();
        for (var i = 0; i < ids.Count; i++)
        {
            var parada = await _store.GetParadaByIdAsync(ids[i].Trim(), ct).ConfigureAwait(false)
                         ?? throw new NotFoundException($"Parada '{ids[i]}' no encontrada.");
            list.Add(new ParadaEnRutaDoc
            {
                ParadaId = parada.Id,
                Nombre = parada.Nombre,
                Latitud = parada.Ubicacion.Latitud,
                Longitud = parada.Ubicacion.Longitud,
                OrdenSecuencia = i + 1,
            });
        }

        return list;
    }

    private static void ValidateCoordinates(double lat, double lon)
    {
        if (lat is < -90 or > 90 || lon is < -180 or > 180)
        {
            throw new RequestValidationException("Coordenadas geográficas fuera de rango válido.");
        }
    }

    private static void ValidateRutaRequest(RutaUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo)
            || string.IsNullOrWhiteSpace(request.Nombre)
            || string.IsNullOrWhiteSpace(request.ParadaOrigen)
            || string.IsNullOrWhiteSpace(request.ParadaDestino))
        {
            throw new RequestValidationException("Código, nombre, paradaOrigen y paradaDestino son obligatorios.");
        }

        if (request.TarifaBaseMxn <= 0 || request.FrecuenciaMinutos <= 0)
        {
            throw new RequestValidationException("Tarifa base y frecuencia deben ser mayores a cero.");
        }
    }

    private static void ValidateHorarioRequest(HorarioUpsertRequest request)
    {
        if (request.RutaId == Guid.Empty || string.IsNullOrWhiteSpace(request.DiaSemana) || request.FrecuenciaMin <= 0)
        {
            throw new RequestValidationException("RutaId, día de semana y frecuencia > 0 son obligatorios.");
        }

        if (!TimeOnly.TryParse(request.HoraInicio, CultureInfo.InvariantCulture, out var ini)
            || !TimeOnly.TryParse(request.HoraFin, CultureInfo.InvariantCulture, out var fin)
            || ini >= fin)
        {
            throw new RequestValidationException("La hora de inicio debe ser válida y menor que la hora de fin.");
        }
    }

    private static void ValidateTarifaRequest(TarifaUpsertRequest request)
    {
        if (request.RutaId == Guid.Empty || string.IsNullOrWhiteSpace(request.TipoTarifa) || request.MontoMxn <= 0)
        {
            throw new RequestValidationException("RutaId, tipoTarifa y montoMxn > 0 son obligatorios.");
        }
    }

    private static HorarioItemDto MapHorario(HorarioDoc h) =>
        new(h.Id, h.RutaId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.FrecuenciaMin, h.SalidasFijas, h.Activo);

    private static TarifaItemDto MapTarifa(TarifaDoc t) =>
        new(t.Id, t.RutaId, t.TipoTarifa, t.MontoMxn, t.DescuentoEstudiantePct, t.Moneda, t.Vigente, t.Notas);
}
