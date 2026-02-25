using Microsoft.EntityFrameworkCore;
using RouteEvidence.Domain.Entities;

namespace RouteEvidence.Infrastructure.Persistence;

public class RouteEvidenceDbContext : DbContext
{
    public RouteEvidenceDbContext(DbContextOptions<RouteEvidenceDbContext> options)
        : base(options) { }

    public DbSet<RouteEvidenceEntity> RouteEvidences => Set<RouteEvidenceEntity>();
    public DbSet<UnitEntity> Units => Set<UnitEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RouteEvidenceEntity>(entity =>
        {
            entity.ToTable("RouteEvidences");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RouteId).IsRequired();
            entity.Property(e => e.EvidenceType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.RecordedAtUtc).IsRequired();
            entity.Property(e => e.Metadata).HasMaxLength(4000);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.HasIndex(e => e.RouteId);
            entity.HasIndex(e => e.RecordedAtUtc);
        });

        modelBuilder.Entity<UnitEntity>(entity =>
        {
            entity.ToTable("Units");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumberUnit).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.HasIndex(e => e.NumberUnit).IsUnique();
        });
    }
}
