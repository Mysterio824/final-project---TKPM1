using Microsoft.EntityFrameworkCore;
using DevTools.Entities;

namespace DevTools.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tool> Tools { get; set; }

    public DbSet<FavoriteTool> FavoriteTools { get; set; }

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
            .HasKey(ft => new { ft.UserId, ft.ToolId });

        base.OnModelCreating(modelBuilder);
    }
}