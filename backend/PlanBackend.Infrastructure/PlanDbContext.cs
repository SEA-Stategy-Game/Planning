using Microsoft.EntityFrameworkCore;
using PlanBackend.Infrastructure.Entities;

namespace PlanBackend.Infrastructure;

public class PlanDbContext(DbContextOptions<PlanDbContext> options) : DbContext(options)
{
    public DbSet<GamePlanEntity> GamePlans => Set<GamePlanEntity>();
    public DbSet<UnitPlanEntity> UnitPlans => Set<UnitPlanEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GamePlanEntity>()
            .HasIndex(g => new { g.GameId, g.PlayerId, g.Version })
            .IsUnique();

        modelBuilder.Entity<GamePlanEntity>()
            .HasIndex(g => new { g.GameId, g.PlayerId, g.IsActive });

        modelBuilder.Entity<UnitPlanEntity>()
            .HasIndex(u => new { u.GamePlanId, u.UnitId });

        modelBuilder.Entity<UnitPlanEntity>()
            .HasOne<GamePlanEntity>()
            .WithMany()
            .HasForeignKey(u => u.GamePlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
