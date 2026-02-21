using Microsoft.EntityFrameworkCore;
using RouteEvidence.Models;

namespace RouteEvidence.Data;

public class RouteEvidenceDbContext : DbContext
{
    public RouteEvidenceDbContext(DbContextOptions<RouteEvidenceDbContext> options)
        : base(options) { }

    public DbSet<EvidenceCatalog> EvidenceCatalog => Set<EvidenceCatalog>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Evidence> Evidence => Set<Evidence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EvidenceCatalog>(entity =>
        {
            entity.ToTable("EvidenceCatalog");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Type).IsUnique();
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("Units");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Plate).IsUnique();
        });

        modelBuilder.Entity<Evidence>(entity =>
        {
            entity.ToTable("Evidence");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Unit)
                .WithMany(u => u.EvidenceItems)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UnitId);
            entity.HasIndex(e => e.DateTime);
            entity.HasIndex(e => e.IsSynced);
        });
    }
}
