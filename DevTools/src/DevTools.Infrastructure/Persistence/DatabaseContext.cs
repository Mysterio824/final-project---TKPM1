using Microsoft.EntityFrameworkCore;
using DevTools.Domain.Entities;

namespace DevTools.DataAccess.Persistence;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tool> Tools { get; set; }
    public DbSet<FavoriteTool> FavoriteTools { get; set; }
    public DbSet<ToolGroup> ToolGroups { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

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
            .ToTable("ToolGroups", "public")
            .HasKey(e => e.Id);

        base.OnModelCreating(modelBuilder);
    }
}