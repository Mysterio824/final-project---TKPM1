using Microsoft.EntityFrameworkCore;
using DevTools.Domain.Entities;

namespace DevTools.Infrastructure.Persistence;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tool> Tools { get; set; }
    public DbSet<FavoriteTool> FavoriteTools { get; set; }
    public DbSet<ToolGroup> ToolGroups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<User>()
            .ToTable("Users", "public")
            .HasKey(e => e.Id);

        modelBuilder.Entity<Tool>()
            .ToTable("Tools", "public")
            .HasKey(e => e.Id);

        modelBuilder.Entity<FavoriteTool>()
            .ToTable("FavoriteTools", "public")
            .HasKey(e => e.Id);

        modelBuilder.Entity<ToolGroup>()
            .ToTable("ToolGroup", "public")
            .HasKey(ft => ft.Id);

        base.OnModelCreating(modelBuilder);
    }
}