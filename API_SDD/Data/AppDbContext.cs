using ApiSDD.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiSDD.Data;

/// <summary>
/// DbContext nv-01 (SQL externo con retry + timeout 60s) + entidades de Auth y Transporte.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Entidades históricas NV-04
    public DbSet<SDDUsuario> SDDUsuarios => Set<SDDUsuario>();
    public DbSet<SDDSegBloqueo> SDDSegBloqueo => Set<SDDSegBloqueo>();

    // Módulo de Autenticación y Usuarios (TB-AU-01 a TB-AU-04)
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TokenRecuperacion> TokensRecuperacion => Set<TokenRecuperacion>();

    // Módulo de Transporte (TB-EM-01, TB-EM-02 y Rutas)
    public DbSet<MedioTransporte> MediosTransporte => Set<MedioTransporte>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Ruta> Rutas => Set<Ruta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // Mapeo legacy SDDUsuario
        modelBuilder.Entity<SDDUsuario>(entity =>
        {
            entity.ToTable("SDDUsuario");
            entity.HasKey(e => e.id);
            entity.Property(e => e.strNombreUsuario).HasMaxLength(10).IsRequired();
            entity.HasIndex(e => e.strNombreUsuario).IsUnique()
                .HasDatabaseName("IX_SDDUsuario_strNombreUsuario");
            entity.Property(e => e.strCorreoElectronico).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.strCorreoElectronico).IsUnique()
                .HasDatabaseName("IX_SDDUsuario_strCorreoElectronico");
            entity.Property(e => e.strPWD).HasMaxLength(200).IsRequired();
            entity.Property(e => e.strRol).HasMaxLength(20).IsRequired().HasDefaultValue("Usuario");
            entity.Property(e => e.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<SDDSegBloqueo>(entity =>
        {
            entity.ToTable("SDDSegBloqueo");
            entity.HasKey(e => e.strNombreUsuario);
            entity.Property(e => e.strNombreUsuario).HasMaxLength(10).IsRequired();
            entity.Property(e => e.intIntentos).IsRequired();
            entity.Property(e => e.dteBloqueoHasta).IsRequired(false);
            entity.Property(e => e.RowVersion).IsRowVersion();
        });

        // TB-AU-01: roles
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id).HasName("pk_roles");
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("uk_roles_nombre");

            // Seed inicial de roles requeridos
            entity.HasData(
                new Rol { Id = 1, Nombre = "Usuario", Descripcion = "Usuario final del sistema", FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Rol { Id = 2, Nombre = "Administrador", Descripcion = "Administrador del sistema", FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        // TB-AU-02: usuarios
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id).HasName("pk_usuarios");
            entity.Property(e => e.NombreCompleto).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Correo).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Correo).IsUnique().HasDatabaseName("uk_usuarios_correo");
            entity.HasIndex(e => new { e.RolId, e.Activo, e.FechaCreacion }).HasDatabaseName("idx_usuarios_rol_activo");

            entity.HasOne(e => e.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(e => e.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TB-AU-03: refresh_tokens
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id).HasName("pk_refresh_tokens");
            entity.Property(e => e.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(e => e.TokenHash).IsUnique().HasDatabaseName("idx_refresh_tokens_token_hash");
            entity.HasIndex(e => new { e.UsuarioId, e.FechaExpiracion, e.Revocado }).HasDatabaseName("idx_refresh_tokens_usuario_activos");
            entity.HasIndex(e => e.FechaExpiracion).HasDatabaseName("idx_refresh_tokens_expiracion");

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TB-AU-04: tokens_recuperacion
        modelBuilder.Entity<TokenRecuperacion>(entity =>
        {
            entity.ToTable("tokens_recuperacion");
            entity.HasKey(e => e.Id).HasName("pk_tokens_recuperacion");
            entity.Property(e => e.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(e => e.TokenHash).IsUnique().HasDatabaseName("idx_tokens_recup_hash");
            entity.HasIndex(e => new { e.UsuarioId, e.FechaExpiracion, e.Usado }).HasDatabaseName("idx_tokens_recup_usuario_vigentes");

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.TokensRecuperacion)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TB-EM-01: medios_transporte
        modelBuilder.Entity<MedioTransporte>(entity =>
        {
            entity.ToTable("medios_transporte");
            entity.HasKey(e => e.Id).HasName("pk_medios_transporte");
            entity.Property(e => e.Clave).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(60).IsRequired();
            entity.HasIndex(e => e.Clave).IsUnique().HasDatabaseName("idx_medios_transporte_clave");
            entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("uk_medios_transporte_nombre");
            entity.HasIndex(e => new { e.Activo, e.Nombre }).HasDatabaseName("idx_medios_transporte_activo");
        });

        // TB-EM-02: empresas
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("empresas");
            entity.HasKey(e => e.Id).HasName("pk_empresas");
            entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("uk_empresas_nombre");
            entity.HasIndex(e => new { e.Activo, e.CalificacionPromedio, e.TotalCalificaciones })
                .HasDatabaseName("idx_empresas_activa_calificacion");
            entity.HasIndex(e => e.MunicipioBase).HasDatabaseName("idx_empresas_municipio");
        });

        // rutas
        modelBuilder.Entity<Ruta>(entity =>
        {
            entity.ToTable("rutas");
            entity.HasKey(e => e.Id).HasName("pk_rutas");
            entity.HasIndex(e => new { e.EmpresaId, e.MedioTransporteId }).HasDatabaseName("idx_rutas_empresa_medio");

            entity.HasOne(e => e.Empresa)
                .WithMany(em => em.Rutas)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MedioTransporte)
                .WithMany(m => m.Rutas)
                .HasForeignKey(e => e.MedioTransporteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
