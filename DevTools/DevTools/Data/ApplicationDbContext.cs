// DevTools/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using DevTools.Entities;

namespace DevTools.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema to 'auth'
        modelBuilder.HasDefaultSchema("public");

        // Optional: Configure specific entity schema if different
        modelBuilder.Entity<User>().ToTable("Users", "public");
    }
}