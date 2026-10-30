using ApiSDD.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiSDD.Data;

/// <summary>
/// DbContext nv-01 (SQL externo con retry + timeout 60s) + primera entidad nv-04.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SDDUsuario> SDDUsuarios => Set<SDDUsuario>();

    public DbSet<SDDSegBloqueo> SDDSegBloqueo => Set<SDDSegBloqueo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
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
        base.OnModelCreating(modelBuilder);
    }
}
