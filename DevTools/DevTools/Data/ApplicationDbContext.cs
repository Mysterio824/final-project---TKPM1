// DevTools/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using DevTools.Entities;

namespace DevTools.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tool> Tools { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.Entity<User>().ToTable("Users", "public");
        modelBuilder.Entity<Tool>().ToTable("Tools", "public");
        base.OnModelCreating(modelBuilder);
    }
}