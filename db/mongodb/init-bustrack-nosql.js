// ============================================================================
// BusTrackNoSQL — Script Idempotente de Creación de Colecciones, Índices y Seed
// Motor: MongoDB Atlas (NoSQL Documental) — Base de datos: BusTrackNoSQL
// Requisitos cubiertos: Rúbrica v3 §14 (NoSQL), §15 (Indexing), §20 (Script BD)
// Uso: mongosh "$MONGODB_CONNECTION_STRING" --file db/mongodb/init-bustrack-nosql.js
// ============================================================================

const dbName = "BusTrackNoSQL";
const database = db.getSiblingDB(dbName);
const now = new Date();

function ensureCollection(name, validator) {
  const exists = database.getCollectionNames().includes(name);
  if (!exists) {
    database.createCollection(name, validator ? { validator } : {});
  } else if (validator) {
    database.runCommand({ collMod: name, validator });
  }
}

// 1. Colecciones Core + Validadores JSON Schema básicos
ensureCollection("roles", {
  $jsonSchema: {
    bsonType: "object",
    required: ["nombre"],
    properties: {
      nombre: { bsonType: "string" }
    }
  }
});

ensureCollection("usuarios", {
  $jsonSchema: {
    bsonType: "object",
    required: ["correo", "nombreCompleto", "passwordHash", "rolNombre", "activo"],
    properties: {
      correo: { bsonType: "string" },
      nombreCompleto: { bsonType: "string" },
      passwordHash: { bsonType: "string" },
      rolNombre: { enum: ["Usuario", "Admin"] },
      activo: { bsonType: "bool" }
    }
  }
});

ensureCollection("refresh_tokens");
ensureCollection("tokens_recuperacion");
ensureCollection("seg_bloqueo");
ensureCollection("mediosTransporte");
ensureCollection("empresas");
ensureCollection("paradas", {
  $jsonSchema: {
    bsonType: "object",
    required: ["nombre", "ubicacion", "activo"],
    properties: {
      nombre: { bsonType: "string" },
      ubicacion: {
        bsonType: "object",
        required: ["type", "coordinates"],
        properties: {
          type: { enum: ["Point"] },
          coordinates: { bsonType: "array", minItems: 2, maxItems: 2 }
        }
      },
      activo: { bsonType: "bool" }
    }
  }
});
ensureCollection("rutas");
ensureCollection("horarios");
ensureCollection("tarifas");
ensureCollection("destinos");
ensureCollection("calificaciones");
ensureCollection("viajes_guardados");
ensureCollection("consentimientos_sensores");
ensureCollection("lotes_sensores");
ensureCollection("candidatos_parada");
ensureCollection("solicitudes_recomendacion");
ensureCollection("fuentes_datos");
ensureCollection("importaciones");
ensureCollection("dispositivos_fcm");
ensureCollection("cache_distribuida");

// 2. Índices Únicos, Compuestos, TTL y Geoespaciales (2dsphere)
database.roles.createIndex({ nombre: 1 }, { name: "ux_roles_nombre", unique: true });
database.usuarios.createIndex({ correo: 1 }, { name: "ux_usuarios_correo", unique: true });
database.refresh_tokens.createIndex({ tokenHash: 1 }, { name: "ux_refresh_hash", unique: true });
database.refresh_tokens.createIndex({ fechaExpiracion: 1 }, { name: "ttl_refresh_expira", expireAfterSeconds: 0 });
database.tokens_recuperacion.createIndex({ tokenHash: 1 }, { name: "ux_rec_hash", unique: true });
database.tokens_recuperacion.createIndex({ fechaExpiracion: 1 }, { name: "ttl_rec_expira", expireAfterSeconds: 0 });

database.mediosTransporte.createIndex({ clave: 1 }, { name: "ux_medios_clave", unique: true });
database.empresas.createIndex({ nombre: "text", siglas: "text", descripcion: "text" }, { name: "txt_empresas_busqueda" });
database.empresas.createIndex({ municipioBase: 1, activo: 1 }, { name: "ix_empresas_municipio_activo" });

database.paradas.createIndex({ ubicacion: "2dsphere" }, { name: "geo_paradas_ubicacion" });
database.paradas.createIndex({ municipio: 1, activo: 1 }, { name: "ix_paradas_municipio" });

database.rutas.createIndex({ empresaId: 1, activa: 1 }, { name: "ix_rutas_empresa_activa" });
database.rutas.createIndex({ nombre: "text", paradaOrigen: "text", paradaDestino: "text" }, { name: "txt_rutas_busqueda" });

database.horarios.createIndex({ rutaId: 1, diaSemana: 1 }, { name: "ix_horarios_ruta_dia" });
database.tarifas.createIndex({ rutaId: 1, vigente: 1 }, { name: "ix_tarifas_ruta_vigente" });

database.destinos.createIndex({ ubicacion: "2dsphere" }, { name: "geo_destinos_ubicacion" });
database.destinos.createIndex({ categoria: 1, activo: 1 }, { name: "ix_destinos_categoria" });

database.calificaciones.createIndex({ rutaId: 1, fechaCreacion: -1 }, { name: "ix_calificaciones_ruta_fecha" });
database.calificaciones.createIndex({ usuarioId: 1, fechaCreacion: -1 }, { name: "ix_calificaciones_usuario" });
database.viajes_guardados.createIndex({ usuarioId: 1, fechaActualizacion: -1 }, { name: "ix_viajes_usuario" });

database.lotes_sensores.createIndex({ rutaId: 1, creadoEnUtc: -1 }, { name: "ix_lotes_ruta_fecha" });
database.candidatos_parada.createIndex({ ubicacion: "2dsphere" }, { name: "geo_candidatos_parada" });
database.candidatos_parada.createIndex({ estado: 1, rutaId: 1 }, { name: "ix_candidatos_estado" });

database.solicitudes_recomendacion.createIndex({ usuarioId: 1, creadoEnUtc: -1 }, { name: "ix_recom_usuario" });
database.fuentes_datos.createIndex({ nombre: 1 }, { name: "ux_fuentes_nombre", unique: true });
database.importaciones.createIndex({ fuenteDatoId: 1, ejecutadoEnUtc: -1 }, { name: "ix_importaciones_fuente" });
database.dispositivos_fcm.createIndex({ tokenFcm: 1 }, { name: "ux_fcm_token", unique: true });
database.cache_distribuida.createIndex({ expiraEnUtc: 1 }, { name: "ttl_cache_expira", expireAfterSeconds: 0 });

// 3. Datos Semilla Idempotentes Alineados con API (UUIDs estándar)
const empAvmId = UUID("20000000-0000-0000-0000-000000000001");
const empTepId = UUID("20000000-0000-0000-0000-000000000002");
const empFlhId = UUID("20000000-0000-0000-0000-000000000003");
const fntId = UUID("30000000-0000-0000-0000-000000000001");
const rut1Id = UUID("10000000-0000-0000-0000-000000000001");
const rut2Id = UUID("10000000-0000-0000-0000-000000000002");
const rut3Id = UUID("10000000-0000-0000-0000-000000000003");
const rut4Id = UUID("10000000-0000-0000-0000-000000000004");

database.roles.bulkWrite([
  { updateOne: { filter: { _id: "ROL-USR" }, update: { $set: { nombre: "Usuario", descripcion: "Usuario pasajero / colaborador" } }, upsert: true } },
  { updateOne: { filter: { _id: "ROL-ADM" }, update: { $set: { nombre: "Admin", descripcion: "Administrador del sistema" } }, upsert: true } }
]);

database.mediosTransporte.bulkWrite([
  { updateOne: { filter: { _id: 1 }, update: { $set: { clave: "BUS-DIR", nombre: "Autobús Directo", descripcion: "Servicio foráneo directo entre terminales", iconoClave: "bus", activo: true, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: 2 }, update: { $set: { clave: "MIC-URB", nombre: "Microbús Urbano", descripcion: "Rutas urbanas regulares locales", iconoClave: "bus-front", activo: true, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: 3 }, update: { $set: { clave: "COM-COL", nombre: "Combi Colectiva", descripcion: "Transporte suburbano intermunicipal", iconoClave: "van", activo: true, fechaActualizacion: now } }, upsert: true } }
]);

database.empresas.bulkWrite([
  { updateOne: { filter: { _id: empAvmId }, update: { $set: { nombre: "Autotransportes Valle del Mezquital", siglas: "AVM", descripcion: "Línea regional Pachuca – Tula – Refinería – Tepeji", municipioBase: "Tula de Allende", telefonoContacto: "773-732-0100", calificacionPromedio: 4.6, totalEvaluaciones: 184, mediosTransporte: ["Autobús Directo", "Microbús Urbano"], activo: true, fuenteDatoId: fntId, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: empTepId }, update: { $set: { nombre: "Autotransportes Tepeji del Río", siglas: "ATTR", descripcion: "Servicio metropolitano Tula – El Carmen UTTT – Tepeji Centro", municipioBase: "Tepeji del Río", telefonoContacto: "773-733-0200", calificacionPromedio: 4.4, totalEvaluaciones: 112, mediosTransporte: ["Microbús Urbano", "Combi Colectiva"], activo: true, fuenteDatoId: fntId, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: empFlhId }, update: { $set: { nombre: "Flecha Roja Hidalgo", siglas: "FRH", descripcion: "Rutas directas hacia corredores turísticos y Pachuca", municipioBase: "Pachuca de Soto", telefonoContacto: "771-715-0300", calificacionPromedio: 4.5, totalEvaluaciones: 95, mediosTransporte: ["Autobús Directo"], activo: true, fuenteDatoId: fntId, fechaActualizacion: now } }, upsert: true } }
]);

database.paradas.bulkWrite([
  { updateOne: { filter: { _id: "PAR-01" }, update: { $set: { nombre: "Central de Autobuses de Pachuca", municipio: "Pachuca de Soto", tipo: "Terminal", referencias: "Blvd. Javier Rojo Gómez", ubicacion: { type: "Point", coordinates: [-98.7523, 20.1012] }, techada: true, rutasDisponibles: 8, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-02" }, update: { $set: { nombre: "Plaza Juárez Centro Pachuca", municipio: "Pachuca de Soto", tipo: "Urbana", referencias: "Centro Histórico", ubicacion: { type: "Point", coordinates: [-98.7321, 20.1245] }, techada: true, rutasDisponibles: 12, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-03" }, update: { $set: { nombre: "Puente Actopan Troncal", municipio: "Actopan", tipo: "Intermedia", referencias: "Carretera México-Laredo", ubicacion: { type: "Point", coordinates: [-98.9431, 20.2687] }, techada: true, rutasDisponibles: 6, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-04" }, update: { $set: { nombre: "Central de Autobuses de Tula", municipio: "Tula de Allende", tipo: "Terminal", referencias: "Av. Hidalgo Centro", ubicacion: { type: "Point", coordinates: [-99.3421, 20.0512] }, techada: true, rutasDisponibles: 10, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-05" }, update: { $set: { nombre: "Crucero Cruz Azul - San Marcos", municipio: "Tula de Allende", tipo: "Escolar", referencias: "Acceso UTTT y Ciudad Cooperativa", ubicacion: { type: "Point", coordinates: [-99.2891, 20.0345] }, techada: true, rutasDisponibles: 4, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-06" }, update: { $set: { nombre: "Jardín Municipal Tepeji del Río", municipio: "Tepeji del Río", tipo: "Terminal", referencias: "Zona Centro Tepeji", ubicacion: { type: "Point", coordinates: [-99.3412, 19.9056] }, techada: true, rutasDisponibles: 5, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-07" }, update: { $set: { nombre: "Monumento al Minero Real del Monte", municipio: "Mineral del Monte", tipo: "Turística", referencias: "Pueblo Mágico", ubicacion: { type: "Point", coordinates: [-98.6712, 20.1412] }, techada: true, rutasDisponibles: 3, activo: true } }, upsert: true } }
]);

database.rutas.bulkWrite([
  { updateOne: { filter: { _id: rut1Id }, update: { $set: { codigo: "R-01", nombre: "Pachuca – Tula Directo Express", empresaId: empAvmId, empresaNombre: "Autotransportes Valle del Mezquital", medioTransporteId: 1, medioTransporteNombre: "Autobús Directo", paradaOrigen: "Central de Autobuses de Pachuca", paradaDestino: "Central de Autobuses de Tula", municipio: "Tula de Allende", tarifaBaseMxn: NumberDecimal("65.00"), duracionEstimadaMin: 70, distanciaKm: 68.5, primerHorario: "05:00", ultimoHorario: "22:30", frecuenciaMinutos: 15, diasOperacion: "Lunes a Domingo", calificacionPromedio: 4.6, totalCalificaciones: 42, activa: true } }, upsert: true } },
  { updateOne: { filter: { _id: rut2Id }, update: { $set: { codigo: "R-02", nombre: "Tula – Tepeji Centro Metropolitano", empresaId: empTepId, empresaNombre: "Autotransportes Tepeji del Río", medioTransporteId: 2, medioTransporteNombre: "Microbús Urbano", paradaOrigen: "Tula Centro (Plaza Juárez)", paradaDestino: "Tepeji Centro (Jardín Municipal)", municipio: "Tepeji del Río", tarifaBaseMxn: NumberDecimal("22.00"), duracionEstimadaMin: 35, distanciaKm: 19.2, primerHorario: "05:30", ultimoHorario: "22:00", frecuenciaMinutos: 10, diasOperacion: "Lunes a Domingo", calificacionPromedio: 4.4, totalCalificaciones: 38, activa: true } }, upsert: true } },
  { updateOne: { filter: { _id: rut3Id }, update: { $set: { codigo: "R-03", nombre: "Tula – Refinería Miguel Hidalgo", empresaId: empAvmId, empresaNombre: "Autotransportes Valle del Mezquital", medioTransporteId: 2, medioTransporteNombre: "Microbús Urbano", paradaOrigen: "Central de Autobuses de Tula", paradaDestino: "Puerta 1 Refinería", municipio: "Tula de Allende", tarifaBaseMxn: NumberDecimal("15.00"), duracionEstimadaMin: 24, distanciaKm: 11.4, primerHorario: "06:00", ultimoHorario: "21:00", frecuenciaMinutos: 12, diasOperacion: "Lunes a Sábado", calificacionPromedio: 4.3, totalCalificaciones: 24, activa: true } }, upsert: true } },
  { updateOne: { filter: { _id: rut4Id }, update: { $set: { codigo: "R-04", nombre: "Pachuca – Mineral del Monte Turístico", empresaId: empFlhId, empresaNombre: "Flecha Roja Hidalgo", medioTransporteId: 1, medioTransporteNombre: "Autobús Directo", paradaOrigen: "Plaza Juárez Pachuca", paradaDestino: "Monumento al Minero Real del Monte", municipio: "Mineral del Monte", tarifaBaseMxn: NumberDecimal("30.00"), duracionEstimadaMin: 30, distanciaKm: 16.0, primerHorario: "06:30", ultimoHorario: "20:00", frecuenciaMinutos: 20, diasOperacion: "Lunes a Domingo", calificacionPromedio: 4.8, totalCalificaciones: 51, activa: true } }, upsert: true } }
]);

database.horarios.bulkWrite([
  { updateOne: { filter: { _id: "HOR-01" }, update: { $set: { rutaId: rut1Id, diaSemana: "LunesAViernes", horaInicio: "05:00", horaFin: "22:30", frecuenciaMin: 15, salidasFijas: ["05:00", "06:00", "07:00", "08:30", "10:00", "12:00", "14:00", "16:30", "18:00", "20:00", "22:00"], activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "HOR-02" }, update: { $set: { rutaId: rut2Id, diaSemana: "LunesADomingo", horaInicio: "05:30", horaFin: "22:00", frecuenciaMin: 10, salidasFijas: ["05:30", "06:30", "07:30", "09:00", "11:00", "13:00", "15:00", "17:00", "19:00", "21:00"], activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "HOR-03" }, update: { $set: { rutaId: rut3Id, diaSemana: "LunesASabado", horaInicio: "06:00", horaFin: "21:00", frecuenciaMin: 12, salidasFijas: ["06:00", "07:00", "08:00", "10:00", "12:30", "15:00", "18:00", "20:30"], activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "HOR-04" }, update: { $set: { rutaId: rut4Id, diaSemana: "LunesADomingo", horaInicio: "06:30", horaFin: "20:00", frecuenciaMin: 20, salidasFijas: ["06:30", "08:00", "09:30", "11:00", "13:00", "15:00", "17:00", "19:30"], activo: true } }, upsert: true } }
]);

database.tarifas.bulkWrite([
  { updateOne: { filter: { _id: "TAR-01" }, update: { $set: { rutaId: rut1Id, tipoTarifa: "Foránea", montoMxn: NumberDecimal("65.00"), descuentoEstudiantePct: 25, moneda: "MXN", vigente: true } }, upsert: true } },
  { updateOne: { filter: { _id: "TAR-02" }, update: { $set: { rutaId: rut2Id, tipoTarifa: "General", montoMxn: NumberDecimal("22.00"), descuentoEstudiantePct: 35, moneda: "MXN", vigente: true } }, upsert: true } },
  { updateOne: { filter: { _id: "TAR-03" }, update: { $set: { rutaId: rut3Id, tipoTarifa: "Urbana", montoMxn: NumberDecimal("15.00"), descuentoEstudiantePct: 50, moneda: "MXN", vigente: true } }, upsert: true } },
  { updateOne: { filter: { _id: "TAR-04" }, update: { $set: { rutaId: rut4Id, tipoTarifa: "Turística", montoMxn: NumberDecimal("30.00"), descuentoEstudiantePct: 20, moneda: "MXN", vigente: true } }, upsert: true } }
]);

database.destinos.bulkWrite([
  { updateOne: { filter: { _id: "DES-01" }, update: { $set: { nombre: "Zona Arqueológica de Tula (Los Atlantes)", categoria: "Arqueológico", municipio: "Tula de Allende", descripcion: "Antigua capital tolteca con esculturas monumentales y museo de sitio.", paradaCercanaId: "PAR-04", rutasIds: [rut1Id, rut2Id, rut3Id], ubicacion: { type: "Point", coordinates: [-99.3404, 20.0642] }, calificacionPromedio: 4.8, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "DES-02" }, update: { $set: { nombre: "Presa Requena y Balneario El Salto", categoria: "Ecoturismo", municipio: "Tepeji del Río", descripcion: "Área natural ideal para caminata, pesca deportiva y convivencia familiar.", paradaCercanaId: "PAR-06", rutasIds: [rut2Id], ubicacion: { type: "Point", coordinates: [-99.3208, 19.9367] }, calificacionPromedio: 4.5, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "DES-03" }, update: { $set: { nombre: "Pueblo Mágico Real del Monte", categoria: "Cultural", municipio: "Mineral del Monte", descripcion: "Arquitectura minera histórica, museos y gastronomía tradicional.", paradaCercanaId: "PAR-07", rutasIds: [rut4Id], ubicacion: { type: "Point", coordinates: [-98.6712, 20.1412] }, calificacionPromedio: 4.9, activo: true } }, upsert: true } }
]);

database.fuentes_datos.bulkWrite([
  { updateOne: { filter: { _id: fntId }, update: { $set: { nombre: "Relevamiento Campo UTTT Hidalgo 2026", tipoFuente: "Oficial / Campo", responsable: "Equipo BusTrack UTTT", activa: true, actualizadaEnUtc: now } }, upsert: true } }
]);

print(`BusTrackNoSQL inicializado correctamente en ${dbName}.`);
