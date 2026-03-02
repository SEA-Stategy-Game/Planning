using Microsoft.EntityFrameworkCore;
using PlanBackend.Infrastructure.Entities;

namespace PlanBackend.Infrastructure;

public class PlanDbContext(DbContextOptions<PlanDbContext> options) : DbContext(options)
{
    public DbSet<GamePlanEntity> GamePlans => Set<GamePlanEntity>();
    public DbSet<UnitPlanEntity> UnitPlans => Set<UnitPlanEntity>();
}
