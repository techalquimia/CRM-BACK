using Microsoft.EntityFrameworkCore;
using WorkflowApi.Domain.Entities;

namespace WorkflowApi.Infrastructure.Persistence;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
        : base(options) { }

    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Evidence> Evidences => Set<Evidence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("Units");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumberUnit).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.HasIndex(e => e.NumberUnit).IsUnique();
        });

        modelBuilder.Entity<Evidence>(entity =>
        {
            entity.ToTable("Evidences");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeEvidence).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.RecordedAtUtc).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UnitId);
        });
    }
}
