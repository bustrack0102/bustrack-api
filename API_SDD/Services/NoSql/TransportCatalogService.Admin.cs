using System.Globalization;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Operational;
using ApiSDD.Dtos.Transport;

namespace ApiSDD.Services.NoSql;

internal sealed partial class TransportCatalogService
{
    public async Task<ParadaDetalleDto> CreateParadaAsync(ParadaUpsertRequest request, CancellationToken ct)
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
        return MapParadaDetalle(doc, []);
    }

    public async Task<ParadaDetalleDto> UpdateParadaAsync(string id, ParadaUpsertRequest request, CancellationToken ct)
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
        return MapParadaDetalle(doc, []);
    }

    public async Task DeactivateParadaAsync(string id, CancellationToken ct)
    {
        var doc = await _store.GetParadaByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Parada no encontrada.");
        doc.Activo = false;
        await _store.UpsertParadaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
    }

    public async Task<RutaDetalleResponse> CreateRutaAsync(RutaUpsertRequest request, CancellationToken ct)
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

        var paradas = await ResolveParadasOrdenadasAsync(request.ParadasOrdenadas, request.ParadasIds, ct)
            .ConfigureAwait(false);
        var primeraParada = paradas.Count > 0 ? paradas[0] : null;
        var ultimaParada = paradas.Count > 0 ? paradas[paradas.Count - 1] : null;
        var (origenId, origenNombre) = await ResolveExtremoAsync(
            request.ParadaOrigenId, request.ParadaOrigen, primeraParada, ct).ConfigureAwait(false);
        var (destinoId, destinoNombre) = await ResolveExtremoAsync(
            request.ParadaDestinoId, request.ParadaDestino, ultimaParada, ct).ConfigureAwait(false);

        var doc = new RutaDoc
        {
            Id = Guid.NewGuid(),
            Codigo = request.Codigo.Trim(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            EmpresaId = empresa.Id,
            EmpresaNombre = empresa.Nombre,
            MedioTransporteId = medio.Id,
            MedioTransporteNombre = medio.Nombre,
            ParadaOrigenId = origenId,
            ParadaDestinoId = destinoId,
            ParadaOrigen = origenNombre,
            ParadaDestino = destinoNombre,
            Municipio = empresa.MunicipioBase,
            TarifaBaseMxn = request.TarifaBaseMxn,
            DuracionEstimadaMin = Math.Max(5, request.DuracionEstimadaMin),
            DistanciaKm = request.DistanciaTotalKm ?? Math.Max(1.0, paradas.Count * 1.8),
            PrimerHorario = string.IsNullOrWhiteSpace(request.PrimerHorario) ? "05:00" : request.PrimerHorario.Trim(),
            UltimoHorario = string.IsNullOrWhiteSpace(request.UltimoHorario) ? "22:00" : request.UltimoHorario.Trim(),
            FrecuenciaMinutos = Math.Max(1, request.FrecuenciaMinutos),
            DiasOperacion = string.IsNullOrWhiteSpace(request.DiasOperacion) ? "Lun-Dom" : request.DiasOperacion.Trim(),
            CalificacionPromedio = 4.5,
            TotalCalificaciones = 0,
            Activa = request.Activa,
            Trazado = request.TrazadoGeoJson ?? request.Coordenadas,
            FuenteDatoId = request.FuenteDatoId,
            FechaActualizacion = DateTime.UtcNow,
            Paradas = paradas,
        };

        await _store.UpsertRutaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return await BuildRutaDetalleAsync(doc, ct).ConfigureAwait(false);
    }

    public async Task<RutaDetalleResponse> UpdateRutaAsync(Guid id, RutaUpsertRequest request, CancellationToken ct)
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

        var paradas = await ResolveParadasOrdenadasAsync(request.ParadasOrdenadas, request.ParadasIds, ct)
            .ConfigureAwait(false);
        if (paradas.Count > 0)
        {
            doc.Paradas = paradas;
        }

        var primeraParada = doc.Paradas.Count > 0 ? doc.Paradas[0] : null;
        var ultimaParada = doc.Paradas.Count > 0 ? doc.Paradas[doc.Paradas.Count - 1] : null;
        var (origenId, origenNombre) = await ResolveExtremoAsync(
            request.ParadaOrigenId, request.ParadaOrigen,
            primeraParada, ct).ConfigureAwait(false);
        var (destinoId, destinoNombre) = await ResolveExtremoAsync(
            request.ParadaDestinoId, request.ParadaDestino,
            ultimaParada, ct).ConfigureAwait(false);

        doc.Codigo = request.Codigo.Trim();
        doc.Nombre = request.Nombre.Trim();
        doc.Descripcion = request.Descripcion?.Trim() ?? doc.Descripcion;
        doc.EmpresaId = empresa.Id;
        doc.EmpresaNombre = empresa.Nombre;
        doc.MedioTransporteId = medio.Id;
        doc.MedioTransporteNombre = medio.Nombre;
        doc.ParadaOrigenId = origenId;
        doc.ParadaDestinoId = destinoId;
        doc.ParadaOrigen = origenNombre;
        doc.ParadaDestino = destinoNombre;
        doc.TarifaBaseMxn = request.TarifaBaseMxn;
        doc.PrimerHorario = string.IsNullOrWhiteSpace(request.PrimerHorario) ? doc.PrimerHorario : request.PrimerHorario.Trim();
        doc.UltimoHorario = string.IsNullOrWhiteSpace(request.UltimoHorario) ? doc.UltimoHorario : request.UltimoHorario.Trim();
        doc.FrecuenciaMinutos = Math.Max(1, request.FrecuenciaMinutos);
        doc.DiasOperacion = string.IsNullOrWhiteSpace(request.DiasOperacion) ? doc.DiasOperacion : request.DiasOperacion.Trim();
        doc.DuracionEstimadaMin = Math.Max(5, request.DuracionEstimadaMin);
        doc.DistanciaKm = request.DistanciaTotalKm ?? doc.DistanciaKm;
        if (request.TrazadoGeoJson is { Count: > 0 } trazado)
        {
            doc.Trazado = trazado;
        }
        else if (request.Coordenadas is { Count: > 0 } coordenadas)
        {
            doc.Trazado = coordenadas;
        }

        if (request.FuenteDatoId.HasValue)
        {
            doc.FuenteDatoId = request.FuenteDatoId;
        }

        doc.Activa = request.Activa;
        doc.FechaActualizacion = DateTime.UtcNow;

        await _store.UpsertRutaAsync(doc, ct).ConfigureAwait(false);
        await InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return await BuildRutaDetalleAsync(doc, ct).ConfigureAwait(false);
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
        IReadOnlyList<RutaParadaOrdenRequest>? ordenadas,
        IReadOnlyList<string>? ids,
        CancellationToken ct)
    {
        if (ordenadas is { Count: > 0 })
        {
            var ordered = new List<ParadaEnRutaDoc>();
            foreach (var item in ordenadas.OrderBy(o => o.Orden))
            {
                var parada = await _store.GetParadaByIdAsync(item.ParadaId.Trim(), ct).ConfigureAwait(false)
                             ?? throw new NotFoundException($"Parada '{item.ParadaId}' no encontrada.");
                ordered.Add(new ParadaEnRutaDoc
                {
                    ParadaId = parada.Id,
                    Nombre = parada.Nombre,
                    ReferenciaVisual = parada.Referencias,
                    Municipio = parada.Municipio,
                    Latitud = parada.Ubicacion.Latitud,
                    Longitud = parada.Ubicacion.Longitud,
                    OrdenSecuencia = item.Orden > 0 ? item.Orden : ordered.Count + 1,
                    TiempoDesdeOrigenMin = Math.Max(0, item.TiempoMin ?? 0),
                    DistanciaDesdeOrigenKm = Math.Max(0, item.DistanciaKm ?? 0),
                    PermiteAscenso = item.PermiteAscenso,
                    PermiteDescenso = item.PermiteDescenso,
                });
            }

            return ordered;
        }

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
                ReferenciaVisual = parada.Referencias,
                Municipio = parada.Municipio,
                Latitud = parada.Ubicacion.Latitud,
                Longitud = parada.Ubicacion.Longitud,
                OrdenSecuencia = i + 1,
            });
        }

        return list;
    }

    private async Task<(string? Id, string Nombre)> ResolveExtremoAsync(
        string? paradaId,
        string? nombre,
        ParadaEnRutaDoc? fallback,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(paradaId))
        {
            var parada = await _store.GetParadaByIdAsync(paradaId.Trim(), ct).ConfigureAwait(false)
                         ?? throw new NotFoundException($"Parada '{paradaId}' no encontrada.");
            return (parada.Id, parada.Nombre);
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            return (fallback?.ParadaId, nombre.Trim());
        }

        return fallback is null
            ? (null, string.Empty)
            : (fallback.ParadaId, fallback.Nombre);
    }

    private async Task<RutaDetalleResponse> BuildRutaDetalleAsync(RutaDoc doc, CancellationToken ct)
    {
        return await GetRutaByIdAsync(doc.Id, ct).ConfigureAwait(false)
            ?? new RutaDetalleResponse(
                doc.Id,
                doc.Codigo,
                doc.Nombre,
                doc.Descripcion,
                Math.Round(doc.DistanciaKm, 2),
                doc.DuracionEstimadaMin,
                new RutaDetalleEmpresaDto(doc.EmpresaId, doc.EmpresaNombre, doc.CalificacionPromedio),
                new RutaDetalleMedioTransporteDto(doc.MedioTransporteId, doc.MedioTransporteNombre),
                null,
                null,
                BuildStopSequence(doc, [], null, null),
                [],
                [],
                [],
                doc.FechaActualizacion ?? _store.CatalogUpdatedAtUtc);
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
        var tieneOrigen = !string.IsNullOrWhiteSpace(request.ParadaOrigen)
            || !string.IsNullOrWhiteSpace(request.ParadaOrigenId);
        var tieneDestino = !string.IsNullOrWhiteSpace(request.ParadaDestino)
            || !string.IsNullOrWhiteSpace(request.ParadaDestinoId);

        if (string.IsNullOrWhiteSpace(request.Codigo)
            || string.IsNullOrWhiteSpace(request.Nombre)
            || !tieneOrigen
            || !tieneDestino)
        {
            throw new RequestValidationException(
                "Código, nombre, parada de origen y parada de destino son obligatorios.");
        }

        if (request.FrecuenciaMinutos <= 0)
        {
            throw new RequestValidationException("La frecuencia debe ser mayor a cero.");
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
}
