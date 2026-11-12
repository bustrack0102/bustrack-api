namespace ApiSDD.Data.NoSql;

/// <summary>
/// Catálogo semilla unificado Tula–Tepeji / Pachuca compatible con `init-bustrack-nosql.js` y `V1EndpointsTests`.
/// </summary>
internal static class BusTrackSeedData
{
    private static readonly DateTime SeedTimestamp = new(2026, 11, 11, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Guid EmpresaAvmId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid EmpresaTepejiId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid EmpresaFlechaRojaId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid FuenteOficialId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    public static readonly Guid Ruta1Id = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid Ruta2Id = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid Ruta3Id = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid Ruta4Id = Guid.Parse("10000000-0000-0000-0000-000000000004");

    public static IReadOnlyList<RolDoc> Roles() =>
    [
        new() { Id = "ROL-USR", Nombre = "Usuario", Descripcion = "Usuario pasajero / colaborador" },
        new() { Id = "ROL-ADM", Nombre = "Admin", Descripcion = "Administrador del sistema" },
    ];

    public static IReadOnlyList<MedioTransporteDoc> MediosTransporte() =>
    [
        new() { Id = 1, Clave = "BUS-DIR", Nombre = "Autobús Directo", Descripcion = "Servicio foráneo directo entre terminales", IconoClave = "bus", Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = 2, Clave = "MIC-URB", Nombre = "Microbús Urbano", Descripcion = "Rutas urbanas regulares locales", IconoClave = "bus-front", Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = 3, Clave = "COM-COL", Nombre = "Combi Colectiva", Descripcion = "Transporte suburbano intermunicipal", IconoClave = "van", Activo = true, FechaActualizacion = SeedTimestamp },
    ];

    public static IReadOnlyList<EmpresaDoc> Empresas() =>
    [
        new()
        {
            Id = EmpresaAvmId,
            Nombre = "Autotransportes Valle del Mezquital",
            Siglas = "AVM",
            Descripcion = "Línea regional Pachuca – Tula – Refinería – Tepeji",
            MunicipioBase = "Tula de Allende",
            TelefonoContacto = "773-732-0100",
            SitioWeb = "https://avm.bustrack.local",
            CalificacionPromedio = 4.6,
            TotalEvaluaciones = 184,
            MediosTransporte = ["Autobús Directo", "Microbús Urbano"],
            Activo = true,
            FuenteDatoId = FuenteOficialId,
            FechaActualizacion = SeedTimestamp,
        },
        new()
        {
            Id = EmpresaTepejiId,
            Nombre = "Autotransportes Tepeji del Río",
            Siglas = "ATTR",
            Descripcion = "Servicio metropolitano Tula – El Carmen UTTT – Tepeji Centro",
            MunicipioBase = "Tepeji del Río",
            TelefonoContacto = "773-733-0200",
            CalificacionPromedio = 4.4,
            TotalEvaluaciones = 112,
            MediosTransporte = ["Microbús Urbano", "Combi Colectiva"],
            Activo = true,
            FuenteDatoId = FuenteOficialId,
            FechaActualizacion = SeedTimestamp,
        },
        new()
        {
            Id = EmpresaFlechaRojaId,
            Nombre = "Flecha Roja Hidalgo",
            Siglas = "FRH",
            Descripcion = "Rutas directas hacia corredores turísticos y Pachuca",
            MunicipioBase = "Pachuca de Soto",
            TelefonoContacto = "771-715-0300",
            CalificacionPromedio = 4.5,
            TotalEvaluaciones = 95,
            MediosTransporte = ["Autobús Directo"],
            Activo = true,
            FuenteDatoId = FuenteOficialId,
            FechaActualizacion = SeedTimestamp,
        },
    ];

    public static IReadOnlyList<ParadaDoc> Paradas() =>
    [
        new() { Id = "PAR-01", Codigo = "P-01", Nombre = "Central de Autobuses de Pachuca", Municipio = "Pachuca de Soto", Tipo = "Terminal", EsTerminal = true, Referencias = "Blvd. Javier Rojo Gómez", Ubicacion = GeoJsonPointDoc.FromLatLon(20.1012, -98.7523), Techada = true, RutasDisponibles = 8, Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = "PAR-02", Codigo = "P-02", Nombre = "Plaza Juárez Centro Pachuca", Municipio = "Pachuca de Soto", Tipo = "Urbana", EsTerminal = false, Referencias = "Centro Histórico", Ubicacion = GeoJsonPointDoc.FromLatLon(20.1245, -98.7321), Techada = true, RutasDisponibles = 12, Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = "PAR-03", Codigo = "P-03", Nombre = "Puente Actopan Troncal", Municipio = "Actopan", Tipo = "Intermedia", EsTerminal = false, Referencias = "Carretera México-Laredo", Ubicacion = GeoJsonPointDoc.FromLatLon(20.2687, -98.9431), Techada = true, RutasDisponibles = 6, Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = "PAR-04", Codigo = "P-04", Nombre = "Central de Autobuses de Tula", Municipio = "Tula de Allende", Tipo = "Terminal", EsTerminal = true, Referencias = "Av. Hidalgo Centro", Ubicacion = GeoJsonPointDoc.FromLatLon(20.0512, -99.3421), Techada = true, RutasDisponibles = 10, Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = "PAR-05", Codigo = "P-05", Nombre = "Crucero Cruz Azul - San Marcos", Municipio = "Tula de Allende", Tipo = "Escolar", EsTerminal = false, Referencias = "Acceso UTTT y Ciudad Cooperativa", Ubicacion = GeoJsonPointDoc.FromLatLon(20.0345, -99.2891), Techada = true, RutasDisponibles = 4, Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = "PAR-06", Codigo = "P-06", Nombre = "Jardín Municipal Tepeji del Río", Municipio = "Tepeji del Río", Tipo = "Terminal", EsTerminal = true, Referencias = "Zona Centro Tepeji", Ubicacion = GeoJsonPointDoc.FromLatLon(19.9056, -99.3412), Techada = true, RutasDisponibles = 5, Activo = true, FechaActualizacion = SeedTimestamp },
        new() { Id = "PAR-07", Codigo = "P-07", Nombre = "Monumento al Minero Real del Monte", Municipio = "Mineral del Monte", Tipo = "Turística", EsTerminal = false, Referencias = "Pueblo Mágico", Ubicacion = GeoJsonPointDoc.FromLatLon(20.1412, -98.6712), Techada = true, RutasDisponibles = 3, Activo = true, FechaActualizacion = SeedTimestamp },
    ];

    public static IReadOnlyList<RutaDoc> Rutas()
    {
        IReadOnlyList<ParadaEnRutaDoc> seqPachucaTula =
        [
            new() { ParadaId = "PAR-01", Nombre = "Central de Autobuses de Pachuca", ReferenciaVisual = "Blvd. Javier Rojo Gómez", Municipio = "Pachuca de Soto", Latitud = 20.1012, Longitud = -98.7523, OrdenSecuencia = 1, TiempoDesdeOrigenMin = 0, DistanciaDesdeOrigenKm = 0, PermiteAscenso = true, PermiteDescenso = true },
            new() { ParadaId = "PAR-02", Nombre = "Plaza Juárez Centro Pachuca", ReferenciaVisual = "Centro Histórico", Municipio = "Pachuca de Soto", Latitud = 20.1245, Longitud = -98.7321, OrdenSecuencia = 2, TiempoDesdeOrigenMin = 10, DistanciaDesdeOrigenKm = 3.2, PermiteAscenso = true, PermiteDescenso = true },
            new() { ParadaId = "PAR-03", Nombre = "Puente Actopan Troncal", ReferenciaVisual = "Carretera México-Laredo", Municipio = "Actopan", Latitud = 20.2687, Longitud = -98.9431, OrdenSecuencia = 3, TiempoDesdeOrigenMin = 25, DistanciaDesdeOrigenKm = 20.5, PermiteAscenso = true, PermiteDescenso = true },
            new() { ParadaId = "PAR-05", Nombre = "Crucero Cruz Azul - San Marcos", ReferenciaVisual = "Acceso UTTT y Ciudad Cooperativa", Municipio = "Tula de Allende", Latitud = 20.0345, Longitud = -99.2891, OrdenSecuencia = 4, TiempoDesdeOrigenMin = 55, DistanciaDesdeOrigenKm = 52.0, PermiteAscenso = true, PermiteDescenso = true },
            new() { ParadaId = "PAR-04", Nombre = "Central de Autobuses de Tula", ReferenciaVisual = "Av. Hidalgo Centro", Municipio = "Tula de Allende", Latitud = 20.0512, Longitud = -99.3421, OrdenSecuencia = 5, TiempoDesdeOrigenMin = 70, DistanciaDesdeOrigenKm = 68.5, PermiteAscenso = true, PermiteDescenso = true },
        ];

        return
        [
            new()
            {
                Id = Ruta1Id,
                Codigo = "R-01",
                Nombre = "Pachuca – Tula Directo Express",
                Descripcion = "Corredor foráneo directo Pachuca – Actopan – Tula.",
                EmpresaId = EmpresaAvmId,
                EmpresaNombre = "Autotransportes Valle del Mezquital",
                MedioTransporteId = 1,
                MedioTransporteNombre = "Autobús Directo",
                ParadaOrigenId = "PAR-01",
                ParadaDestinoId = "PAR-04",
                ParadaOrigen = "Central de Autobuses de Pachuca",
                ParadaDestino = "Central de Autobuses de Tula",
                Municipio = "Tula de Allende",
                TarifaBaseMxn = 65.00m,
                DuracionEstimadaMin = 70,
                DistanciaKm = 68.5,
                PrimerHorario = "05:00",
                UltimoHorario = "22:30",
                FrecuenciaMinutos = 15,
                DiasOperacion = "Lunes a Domingo",
                CalificacionPromedio = 4.6,
                TotalCalificaciones = 42,
                Activa = true,
                FuenteDatoId = FuenteOficialId,
                FechaActualizacion = SeedTimestamp,
                Trazado =
                [
                    [-98.7523, 20.1012],
                    [-98.7321, 20.1245],
                    [-98.9431, 20.2687],
                    [-99.2891, 20.0345],
                    [-99.3421, 20.0512],
                ],
                Paradas = seqPachucaTula,
            },
            new()
            {
                Id = Ruta2Id,
                Codigo = "R-02",
                Nombre = "Tula – Tepeji Centro Metropolitano",
                Descripcion = "Servicio metropolitano Tula – El Carmen UTTT – Tepeji Centro.",
                EmpresaId = EmpresaTepejiId,
                EmpresaNombre = "Autotransportes Tepeji del Río",
                MedioTransporteId = 2,
                MedioTransporteNombre = "Microbús Urbano",
                ParadaOrigenId = "PAR-04",
                ParadaDestinoId = "PAR-06",
                ParadaOrigen = "Central de Autobuses de Tula",
                ParadaDestino = "Jardín Municipal Tepeji del Río",
                Municipio = "Tepeji del Río",
                TarifaBaseMxn = 22.00m,
                DuracionEstimadaMin = 35,
                DistanciaKm = 19.2,
                PrimerHorario = "05:30",
                UltimoHorario = "22:00",
                FrecuenciaMinutos = 10,
                DiasOperacion = "Lunes a Domingo",
                CalificacionPromedio = 4.4,
                TotalCalificaciones = 38,
                Activa = true,
                FuenteDatoId = FuenteOficialId,
                FechaActualizacion = SeedTimestamp,
                Trazado =
                [
                    [-99.3421, 20.0512],
                    [-99.2891, 20.0345],
                    [-99.3412, 19.9056],
                ],
                Paradas =
                [
                    new() { ParadaId = "PAR-04", Nombre = "Central de Autobuses de Tula", ReferenciaVisual = "Av. Hidalgo Centro", Municipio = "Tula de Allende", Latitud = 20.0512, Longitud = -99.3421, OrdenSecuencia = 1, TiempoDesdeOrigenMin = 0, DistanciaDesdeOrigenKm = 0, PermiteAscenso = true, PermiteDescenso = true },
                    new() { ParadaId = "PAR-05", Nombre = "Crucero Cruz Azul - San Marcos", ReferenciaVisual = "Acceso UTTT y Ciudad Cooperativa", Municipio = "Tula de Allende", Latitud = 20.0345, Longitud = -99.2891, OrdenSecuencia = 2, TiempoDesdeOrigenMin = 10, DistanciaDesdeOrigenKm = 6.5, PermiteAscenso = true, PermiteDescenso = true },
                    new() { ParadaId = "PAR-06", Nombre = "Jardín Municipal Tepeji del Río", ReferenciaVisual = "Zona Centro Tepeji", Municipio = "Tepeji del Río", Latitud = 19.9056, Longitud = -99.3412, OrdenSecuencia = 3, TiempoDesdeOrigenMin = 35, DistanciaDesdeOrigenKm = 19.2, PermiteAscenso = true, PermiteDescenso = true },
                ],
            },
            new()
            {
                Id = Ruta3Id,
                Codigo = "R-03",
                Nombre = "Tula – Refinería Miguel Hidalgo",
                Descripcion = "Ruta urbana de apoyo al corredor industrial de Tula.",
                EmpresaId = EmpresaAvmId,
                EmpresaNombre = "Autotransportes Valle del Mezquital",
                MedioTransporteId = 2,
                MedioTransporteNombre = "Microbús Urbano",
                ParadaOrigenId = "PAR-04",
                ParadaDestinoId = "PAR-05",
                ParadaOrigen = "Central de Autobuses de Tula",
                ParadaDestino = "Crucero Cruz Azul - San Marcos",
                Municipio = "Tula de Allende",
                TarifaBaseMxn = 15.00m,
                DuracionEstimadaMin = 24,
                DistanciaKm = 11.4,
                PrimerHorario = "06:00",
                UltimoHorario = "21:00",
                FrecuenciaMinutos = 12,
                DiasOperacion = "Lunes a Sábado",
                CalificacionPromedio = 4.3,
                TotalCalificaciones = 24,
                Activa = true,
                FuenteDatoId = FuenteOficialId,
                FechaActualizacion = SeedTimestamp,
                Trazado =
                [
                    [-99.3421, 20.0512],
                    [-99.2891, 20.0345],
                ],
                Paradas =
                [
                    new() { ParadaId = "PAR-04", Nombre = "Central de Autobuses de Tula", ReferenciaVisual = "Av. Hidalgo Centro", Municipio = "Tula de Allende", Latitud = 20.0512, Longitud = -99.3421, OrdenSecuencia = 1, TiempoDesdeOrigenMin = 0, DistanciaDesdeOrigenKm = 0, PermiteAscenso = true, PermiteDescenso = true },
                    new() { ParadaId = "PAR-05", Nombre = "Crucero Cruz Azul - San Marcos", ReferenciaVisual = "Acceso UTTT y Ciudad Cooperativa", Municipio = "Tula de Allende", Latitud = 20.0345, Longitud = -99.2891, OrdenSecuencia = 2, TiempoDesdeOrigenMin = 24, DistanciaDesdeOrigenKm = 11.4, PermiteAscenso = true, PermiteDescenso = true },
                ],
            },
            new()
            {
                Id = Ruta4Id,
                Codigo = "R-04",
                Nombre = "Pachuca – Mineral del Monte Turístico",
                Descripcion = "Corredor turístico Pachuca – Real del Monte.",
                EmpresaId = EmpresaFlechaRojaId,
                EmpresaNombre = "Flecha Roja Hidalgo",
                MedioTransporteId = 1,
                MedioTransporteNombre = "Autobús Directo",
                ParadaOrigenId = "PAR-02",
                ParadaDestinoId = "PAR-07",
                ParadaOrigen = "Plaza Juárez Centro Pachuca",
                ParadaDestino = "Monumento al Minero Real del Monte",
                Municipio = "Mineral del Monte",
                TarifaBaseMxn = 30.00m,
                DuracionEstimadaMin = 30,
                DistanciaKm = 16.0,
                PrimerHorario = "06:30",
                UltimoHorario = "20:00",
                FrecuenciaMinutos = 20,
                DiasOperacion = "Lunes a Domingo",
                CalificacionPromedio = 4.8,
                TotalCalificaciones = 51,
                Activa = true,
                FuenteDatoId = FuenteOficialId,
                FechaActualizacion = SeedTimestamp,
                Trazado =
                [
                    [-98.7321, 20.1245],
                    [-98.6712, 20.1412],
                ],
                Paradas =
                [
                    new() { ParadaId = "PAR-02", Nombre = "Plaza Juárez Centro Pachuca", ReferenciaVisual = "Centro Histórico", Municipio = "Pachuca de Soto", Latitud = 20.1245, Longitud = -98.7321, OrdenSecuencia = 1, TiempoDesdeOrigenMin = 0, DistanciaDesdeOrigenKm = 0, PermiteAscenso = true, PermiteDescenso = true },
                    new() { ParadaId = "PAR-07", Nombre = "Monumento al Minero Real del Monte", ReferenciaVisual = "Pueblo Mágico", Municipio = "Mineral del Monte", Latitud = 20.1412, Longitud = -98.6712, OrdenSecuencia = 2, TiempoDesdeOrigenMin = 30, DistanciaDesdeOrigenKm = 16.0, PermiteAscenso = true, PermiteDescenso = true },
                ],
            },
        ];
    }

    public static IReadOnlyList<HorarioDoc> Horarios() =>
    [
        new() { Id = "HOR-01", RutaId = Ruta1Id, DiaSemana = "LunesAViernes", HoraInicio = "05:00", HoraFin = "22:30", FrecuenciaMin = 15, SalidasFijas = ["05:00", "06:00", "07:00", "08:30", "10:00", "12:00", "14:00", "16:30", "18:00", "20:00", "22:00"], Activo = true },
        new() { Id = "HOR-02", RutaId = Ruta2Id, DiaSemana = "LunesADomingo", HoraInicio = "05:30", HoraFin = "22:00", FrecuenciaMin = 10, SalidasFijas = ["05:30", "06:30", "07:30", "09:00", "11:00", "13:00", "15:00", "17:00", "19:00", "21:00"], Activo = true },
        new() { Id = "HOR-03", RutaId = Ruta3Id, DiaSemana = "LunesASabado", HoraInicio = "06:00", HoraFin = "21:00", FrecuenciaMin = 12, SalidasFijas = ["06:00", "07:00", "08:00", "10:00", "12:30", "15:00", "18:00", "20:30"], Activo = true },
        new() { Id = "HOR-04", RutaId = Ruta4Id, DiaSemana = "LunesADomingo", HoraInicio = "06:30", HoraFin = "20:00", FrecuenciaMin = 20, SalidasFijas = ["06:30", "08:00", "09:30", "11:00", "13:00", "15:00", "17:00", "19:30"], Activo = true },
    ];

    public static IReadOnlyList<TarifaDoc> Tarifas() =>
    [
        new() { Id = "TAR-01", RutaId = Ruta1Id, TipoTarifa = "Foránea", MontoMxn = 65.00m, DescuentoEstudiantePct = 25, Vigente = true, Notas = "Presentar credencial escolar vigente" },
        new() { Id = "TAR-02", RutaId = Ruta2Id, TipoTarifa = "General", MontoMxn = 22.00m, DescuentoEstudiantePct = 35, Vigente = true, Notas = "Tarifa metropolitana intermedia" },
        new() { Id = "TAR-03", RutaId = Ruta3Id, TipoTarifa = "Urbana", MontoMxn = 15.00m, DescuentoEstudiantePct = 50, Vigente = true, Notas = "Aplica descuento universitario UTTT" },
        new() { Id = "TAR-04", RutaId = Ruta4Id, TipoTarifa = "Turística", MontoMxn = 30.00m, DescuentoEstudiantePct = 20, Vigente = true, Notas = "Servicio directo corredor turístico" },
    ];

    public static IReadOnlyList<DestinoDoc> Destinos() =>
    [
        new()
        {
            Id = "DES-01",
            Nombre = "Zona Arqueológica de Tula (Los Atlantes)",
            Categoria = "Arqueológico",
            Municipio = "Tula de Allende",
            Descripcion = "Antigua capital tolteca con esculturas monumentales y museo de sitio.",
            ParadaCercanaId = "PAR-04",
            RutasIds = [Ruta1Id, Ruta2Id, Ruta3Id],
            Ubicacion = GeoJsonPointDoc.FromLatLon(20.0642, -99.3404),
            CalificacionPromedio = 4.8,
            Activo = true,
        },
        new()
        {
            Id = "DES-02",
            Nombre = "Presa Requena y Balneario El Salto",
            Categoria = "Ecoturismo",
            Municipio = "Tepeji del Río",
            Descripcion = "Área natural ideal para caminata, pesca deportiva y convivencia familiar.",
            ParadaCercanaId = "PAR-06",
            RutasIds = [Ruta2Id],
            Ubicacion = GeoJsonPointDoc.FromLatLon(19.9367, -99.3208),
            CalificacionPromedio = 4.5,
            Activo = true,
        },
        new()
        {
            Id = "DES-03",
            Nombre = "Pueblo Mágico Real del Monte",
            Categoria = "Cultural",
            Municipio = "Mineral del Monte",
            Descripcion = "Arquitectura minera histórica, museos y gastronomía tradicional.",
            ParadaCercanaId = "PAR-07",
            RutasIds = [Ruta4Id],
            Ubicacion = GeoJsonPointDoc.FromLatLon(20.1412, -98.6712),
            CalificacionPromedio = 4.9,
            Activo = true,
        },
    ];

    public static IReadOnlyList<FuenteDatoDoc> FuentesDatos() =>
    [
        new()
        {
            Id = FuenteOficialId,
            Nombre = "Relevamiento Campo UTTT Hidalgo 2026",
            TipoFuente = "Oficial / Campo",
            Responsable = "Equipo BusTrack UTTT",
            ReferenciaUrl = "uttt.edu.mx",
            Activa = true,
            ActualizadaEnUtc = SeedTimestamp,
        },
    ];
}
