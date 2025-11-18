using Microsoft.EntityFrameworkCore;
using AquaPP.Core.Models.IoT;

namespace AquaPP.Data;

public class ApplicationDbContext : DbContext
{
    // IoT Entities
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<Sensor> Sensors { get; set; } = null!;
    public DbSet<Reading> Readings { get; set; } = null!;
    public DbSet<Alert> Alerts { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Asset configuration
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
        });

        // Sensor configuration
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.AssetId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            
            entity.HasOne(e => e.Asset)
                .WithMany(a => a.Sensors)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Reading configuration
        modelBuilder.Entity<Reading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SensorId, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            
            entity.HasOne(e => e.Sensor)
                .WithMany(s => s.Readings)
                .HasForeignKey(e => e.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Alert configuration
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.SensorId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Acknowledged);
            
            entity.HasOne(e => e.Sensor)
                .WithMany(s => s.Alerts)
                .HasForeignKey(e => e.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
