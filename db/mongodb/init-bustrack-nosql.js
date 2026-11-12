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

// 2. Índices Únicos, Compuestos, TTL y Geoespaciales (2dsphere)
database.roles.createIndex({ nombre: 1 }, { name: "ux_roles_nombre", unique: true });
database.usuarios.createIndex({ correo: 1 }, { name: "ux_usuarios_correo", unique: true });
database.refresh_tokens.createIndex({ tokenHash: 1 }, { name: "ux_refresh_hash", unique: true });
database.refresh_tokens.createIndex({ expiraEnUtc: 1 }, { name: "ttl_refresh_expira", expireAfterSeconds: 0 });
database.tokens_recuperacion.createIndex({ tokenHash: 1 }, { name: "ux_rec_hash", unique: true });
database.tokens_recuperacion.createIndex({ expiraEnUtc: 1 }, { name: "ttl_rec_expira", expireAfterSeconds: 0 });

database.mediosTransporte.createIndex({ clave: 1 }, { name: "ux_medios_clave", unique: true });
database.empresas.createIndex({ nombre: "text", siglas: "text", descripcion: "text" }, { name: "txt_empresas_busqueda" });
database.empresas.createIndex({ municipioBase: 1, activo: 1 }, { name: "ix_empresas_municipio_activo" });

database.paradas.createIndex({ ubicacion: "2dsphere" }, { name: "geo_paradas_ubicacion" });
database.paradas.createIndex({ municipio: 1, activo: 1 }, { name: "ix_paradas_municipio" });

database.rutas.createIndex({ empresaId: 1, activa: 1 }, { name: "ix_rutas_empresa_activa" });
database.rutas.createIndex({ nombre: "text", origenNombre: "text", destinoNombre: "text" }, { name: "txt_rutas_busqueda" });

database.horarios.createIndex({ rutaId: 1, diaSemana: 1 }, { name: "ix_horarios_ruta_dia" });
database.tarifas.createIndex({ rutaId: 1, vigente: 1 }, { name: "ix_tarifas_ruta_vigente" });

database.destinos.createIndex({ ubicacion: "2dsphere" }, { name: "geo_destinos_ubicacion" });
database.destinos.createIndex({ categoria: 1, activo: 1 }, { name: "ix_destinos_categoria" });

database.calificaciones.createIndex({ rutaId: 1, fechaCreacion: -1 }, { name: "ix_calificaciones_ruta_fecha" });
database.calificaciones.createIndex({ usuarioId: 1, fechaCreacion: -1 }, { name: "ix_calificaciones_usuario" });
database.viajes_guardados.createIndex({ usuarioId: 1, fechaActualizacion: -1 }, { name: "ix_viajes_usuario" });

database.consentimientos_sensores.createIndex({ usuarioId: 1 }, { name: "ux_consentimiento_usuario", unique: true });
database.lotes_sensores.createIndex({ rutaId: 1, creadoEnUtc: -1 }, { name: "ix_lotes_ruta_fecha" });
database.candidatos_parada.createIndex({ ubicacion: "2dsphere" }, { name: "geo_candidatos_parada" });
database.candidatos_parada.createIndex({ estado: 1, rutaId: 1 }, { name: "ix_candidatos_estado" });

database.solicitudes_recomendacion.createIndex({ usuarioId: 1, creadoEnUtc: -1 }, { name: "ix_recom_usuario" });
database.fuentes_datos.createIndex({ nombre: 1 }, { name: "ux_fuentes_nombre", unique: true });
database.importaciones.createIndex({ fuenteDatoId: 1, ejecutadoEnUtc: -1 }, { name: "ix_importaciones_fuente" });
database.dispositivos_fcm.createIndex({ tokenFcm: 1 }, { name: "ux_fcm_token", unique: true });

// 3. Datos Semilla Reales Idempotentes (Corredor Tula–Tepeji / Hidalgo)
database.roles.bulkWrite([
  { updateOne: { filter: { _id: "ROL-USR" }, update: { $set: { nombre: "Usuario", descripcion: "Pasajero / colaborador" } }, upsert: true } },
  { updateOne: { filter: { _id: "ROL-ADM" }, update: { $set: { nombre: "Admin", descripcion: "Administrador operativo" } }, upsert: true } }
]);

database.mediosTransporte.bulkWrite([
  { updateOne: { filter: { _id: "MED-01" }, update: { $set: { clave: "BUS-DIR", nombre: "Autobús Directo", descripcion: "Servicio regional entre terminales", iconoClave: "bus", empresaId: "EMP-01", categoria: "Autobús", activo: true, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: "MED-02" }, update: { $set: { clave: "MIC-URB", nombre: "Microbús Urbano", descripcion: "Rutas urbanas regulares locales", iconoClave: "bus-front", empresaId: "EMP-02", categoria: "Microbús", activo: true, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: "MED-03" }, update: { $set: { clave: "COM-COL", nombre: "Combi Colectiva", descripcion: "Transporte suburbano intermunicipal", iconoClave: "van", empresaId: "EMP-03", categoria: "Combi", activo: true, fechaActualizacion: now } }, upsert: true } }
]);

database.empresas.bulkWrite([
  { updateOne: { filter: { _id: "EMP-01" }, update: { $set: { nombre: "Autotransportes Valle del Mezquital", siglas: "AVM", descripcion: "Cobertura Tula, Tepeji, Actopan, Pachuca y CDMX Norte", municipioBase: "Tula de Allende", calificacionPromedio: 4.5, totalEvaluaciones: 184, activo: true, mediosTransporte: ["Autobús Directo"], totalRutasActivas: 2, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: "EMP-02" }, update: { $set: { nombre: "Líneas Urbanas de Tula y El Carmen", siglas: "LUTC", descripcion: "Conectividad urbana Central Tula — UTTT — Cruz Azul", municipioBase: "Tula de Allende", calificacionPromedio: 4.3, totalEvaluaciones: 96, activo: true, mediosTransporte: ["Microbús Urbano", "Combi Colectiva"], totalRutasActivas: 1, fechaActualizacion: now } }, upsert: true } },
  { updateOne: { filter: { _id: "EMP-03" }, update: { $set: { nombre: "Transportes Regionales Tepeji del Río", siglas: "TRT", descripcion: "Conexiones Tepeji, Parque Industrial, UTTT y Tula", municipioBase: "Tepeji del Río", calificacionPromedio: 4.2, totalEvaluaciones: 72, activo: true, mediosTransporte: ["Autobús Directo", "Combi Colectiva"], totalRutasActivas: 1, fechaActualizacion: now } }, upsert: true } }
]);

database.paradas.bulkWrite([
  { updateOne: { filter: { _id: "PAR-01" }, update: { $set: { nombre: "Central de Autobuses Tula", municipio: "Tula de Allende", tipo: "Terminal", ubicacion: { type: "Point", coordinates: [-99.3442, 20.0531] }, referencias: "Frente a Plaza del Nacionalismo", activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-02" }, update: { $set: { nombre: "Campus UTTT El Carmen", municipio: "Tula de Allende", tipo: "Escolar", ubicacion: { type: "Point", coordinates: [-99.3105, 20.0089] }, referencias: "Acceso principal Av. Universidad Tecnológica", activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-03" }, update: { $set: { nombre: "Ciudad Cooperativa Cruz Azul", municipio: "Tula de Allende", tipo: "Intermedia", ubicacion: { type: "Point", coordinates: [-99.3271, 20.0174] }, referencias: "Glorieta Estadio 10 de Diciembre", activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-04" }, update: { $set: { nombre: "Zona Arqueológica de Tula (Los Atlantes)", municipio: "Tula de Allende", tipo: "Turística", ubicacion: { type: "Point", coordinates: [-99.3404, 20.0642] }, referencias: "Entrada Museo Jorge R. Acosta", activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-05" }, update: { $set: { nombre: "Terminal Tepeji del Río Centro", municipio: "Tepeji del Río", tipo: "Terminal", ubicacion: { type: "Point", coordinates: [-99.3414, 19.9039] }, referencias: "Jardín Municipal Tepeji", activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "PAR-06" }, update: { $set: { nombre: "Central de Autobuses Pachuca", municipio: "Pachuca de Soto", tipo: "Terminal", ubicacion: { type: "Point", coordinates: [-98.7654, 20.0975] }, referencias: "Blvd. Javier Rojo Gómez", activo: true } }, upsert: true } }
]);

database.rutas.bulkWrite([
  {
    updateOne: {
      filter: { _id: "10000000-0000-0000-0000-000000000001" },
      update: {
        $set: {
          codigo: "RUT-TUL-UTTT",
          nombre: "Corredor Tula Centro — Campus UTTT",
          empresaId: "EMP-02",
          empresaNombre: "Líneas Urbanas de Tula y El Carmen",
          medioTransporteId: "MED-02",
          medioTransporte: "Microbús Urbano",
          origenNombre: "Central de Autobuses Tula",
          destinoNombre: "Campus UTTT El Carmen",
          duracionEstimadaMin: 28,
          distanciaKm: 11.4,
          tarifaMxn: 12.0,
          calificacionPromedio: 4.6,
          activa: true,
          paradasOrdenadas: [
            { paradaId: "PAR-01", orden: 1, nombre: "Central de Autobuses Tula", coordenadas: [-99.3442, 20.0531], tiempoDesdeOrigenMin: 0 },
            { paradaId: "PAR-03", orden: 2, nombre: "Ciudad Cooperativa Cruz Azul", coordenadas: [-99.3271, 20.0174], tiempoDesdeOrigenMin: 18 },
            { paradaId: "PAR-02", orden: 3, nombre: "Campus UTTT El Carmen", coordenadas: [-99.3105, 20.0089], tiempoDesdeOrigenMin: 28 }
          ],
          geometria: {
            type: "LineString",
            coordinates: [[-99.3442, 20.0531], [-99.3271, 20.0174], [-99.3105, 20.0089]]
          }
        }
      },
      upsert: true
    }
  },
  {
    updateOne: {
      filter: { _id: "10000000-0000-0000-0000-000000000002" },
      update: {
        $set: {
          codigo: "RUT-TEP-UTTT",
          nombre: "Corredor Tepeji Centro — UTTT — Tula",
          empresaId: "EMP-03",
          empresaNombre: "Transportes Regionales Tepeji del Río",
          medioTransporteId: "MED-03",
          medioTransporte: "Combi Colectiva",
          origenNombre: "Terminal Tepeji del Río Centro",
          destinoNombre: "Central de Autobuses Tula",
          duracionEstimadaMin: 42,
          distanciaKm: 23.6,
          tarifaMxn: 18.0,
          calificacionPromedio: 4.3,
          activa: true,
          paradasOrdenadas: [
            { paradaId: "PAR-05", orden: 1, nombre: "Terminal Tepeji del Río Centro", coordenadas: [-99.3414, 19.9039], tiempoDesdeOrigenMin: 0 },
            { paradaId: "PAR-02", orden: 2, nombre: "Campus UTTT El Carmen", coordenadas: [-99.3105, 20.0089], tiempoDesdeOrigenMin: 22 },
            { paradaId: "PAR-01", orden: 3, nombre: "Central de Autobuses Tula", coordenadas: [-99.3442, 20.0531], tiempoDesdeOrigenMin: 42 }
          ],
          geometria: {
            type: "LineString",
            coordinates: [[-99.3414, 19.9039], [-99.3105, 20.0089], [-99.3442, 20.0531]]
          }
        }
      },
      upsert: true
    }
  },
  {
    updateOne: {
      filter: { _id: "10000000-0000-0000-0000-000000000003" },
      update: {
        $set: {
          codigo: "RUT-TUL-PAC",
          nombre: "Directo Tula — Actopan — Pachuca",
          empresaId: "EMP-01",
          empresaNombre: "Autotransportes Valle del Mezquital",
          medioTransporteId: "MED-01",
          medioTransporte: "Autobús Directo",
          origenNombre: "Central de Autobuses Tula",
          destinoNombre: "Central de Autobuses Pachuca",
          duracionEstimadaMin: 75,
          distanciaKm: 68.5,
          tarifaMxn: 68.0,
          calificacionPromedio: 4.5,
          activa: true,
          paradasOrdenadas: [
            { paradaId: "PAR-01", orden: 1, nombre: "Central de Autobuses Tula", coordenadas: [-99.3442, 20.0531], tiempoDesdeOrigenMin: 0 },
            { paradaId: "PAR-06", orden: 2, nombre: "Central de Autobuses Pachuca", coordenadas: [-98.7654, 20.0975], tiempoDesdeOrigenMin: 75 }
          ],
          geometria: {
            type: "LineString",
            coordinates: [[-99.3442, 20.0531], [-98.7654, 20.0975]]
          }
        }
      },
      upsert: true
    }
  }
]);

database.horarios.bulkWrite([
  { updateOne: { filter: { _id: "HOR-01" }, update: { $set: { rutaId: "10000000-0000-0000-0000-000000000001", diaSemana: "LunesAViernes", horaInicio: "05:30", horaFin: "21:30", frecuenciaMin: 10, salidasFijas: ["05:30", "06:00", "06:30", "07:00", "08:00", "12:00", "14:00", "18:00", "20:00"], activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "HOR-02" }, update: { $set: { rutaId: "10000000-0000-0000-0000-000000000002", diaSemana: "LunesAViernes", horaInicio: "05:45", horaFin: "21:00", frecuenciaMin: 15, salidasFijas: ["05:45", "06:30", "07:15", "09:00", "13:00", "16:30", "19:30"], activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "HOR-03" }, update: { $set: { rutaId: "10000000-0000-0000-0000-000000000003", diaSemana: "Todos", horaInicio: "05:00", horaFin: "22:00", frecuenciaMin: 20, salidasFijas: ["05:00", "06:00", "07:00", "08:30", "10:00", "12:00", "15:00", "18:00", "21:00"], activo: true } }, upsert: true } }
]);

database.tarifas.bulkWrite([
  { updateOne: { filter: { _id: "TAR-01" }, update: { $set: { rutaId: "10000000-0000-0000-0000-000000000001", tipoTarifa: "General", montoMxn: 12.0, descuentoEstudiantePct: 50, vigente: true } }, upsert: true } },
  { updateOne: { filter: { _id: "TAR-02" }, update: { $set: { rutaId: "10000000-0000-0000-0000-000000000002", tipoTarifa: "General", montoMxn: 18.0, descuentoEstudiantePct: 33, vigente: true } }, upsert: true } },
  { updateOne: { filter: { _id: "TAR-03" }, update: { $set: { rutaId: "10000000-0000-0000-0000-000000000003", tipoTarifa: "Foránea", montoMxn: 68.0, descuentoEstudiantePct: 25, vigente: true } }, upsert: true } }
]);

database.destinos.bulkWrite([
  { updateOne: { filter: { _id: "DES-01" }, update: { $set: { nombre: "Zona Arqueológica de Tula (Los Atlantes)", categoria: "Arqueológico", municipio: "Tula de Allende", descripcion: "Capital tolteca con esculturas monumentales e historia prehispánica.", paradaCercanaId: "PAR-04", rutasIds: ["10000000-0000-0000-0000-000000000001"], ubicacion: { type: "Point", coordinates: [-99.3404, 20.0642] }, calificacionPromedio: 4.8, activo: true } }, upsert: true } },
  { updateOne: { filter: { _id: "DES-02" }, update: { $set: { nombre: "Presa Requena y Balneario El Salto", categoria: "Ecoturismo", municipio: "Tepeji del Río", descripcion: "Espacio natural ideal para paseo en lancha y senderismo familiar.", paradaCercanaId: "PAR-05", rutasIds: ["10000000-0000-0000-0000-000000000002"], ubicacion: { type: "Point", coordinates: [-99.3208, 19.9367] }, calificacionPromedio: 4.5, activo: true } }, upsert: true } }
]);

database.fuentes_datos.bulkWrite([
  { updateOne: { filter: { _id: "FNT-01" }, update: { $set: { nombre: "Relevamiento Campo UTTT Hidalgo 2026", tipoFuente: "Oficial / Campo", responsable: "Equipo BusTrack UTTT", activa: true, actualizadaEnUtc: now } }, upsert: true } }
]);

print(`BusTrackNoSQL inicializado correctamente en ${dbName}.`);
