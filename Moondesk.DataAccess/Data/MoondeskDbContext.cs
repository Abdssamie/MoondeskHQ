using Microsoft.EntityFrameworkCore;
using Moondesk.Domain.Models;
using Moondesk.Domain.Models.IoT;
using Moondesk.Domain.Models.Network;

namespace Moondesk.DataAccess.Data;

public class MoondeskDbContext : DbContext
{
    public MoondeskDbContext(DbContextOptions<MoondeskDbContext> options) : base(options) { }

    // Core entities
    public DbSet<User> Users { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationMembership> OrganizationMemberships { get; set; }
    public DbSet<ConnectionCredential> ConnectionCredentials { get; set; }

    // IoT entities with organization extensions
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Command> Commands { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUserEntities(modelBuilder);
        ConfigureIoTEntities(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureTimescaleDb(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will only be used if no options are provided
            optionsBuilder.UseNpgsql("Host=localhost;Database=moondesk;Username=postgres;Password=password");
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }

    private void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationMembership>()
            .HasKey(om => new { om.UserId, om.OrganizationId });

        modelBuilder.Entity<OrganizationMembership>()
            .HasOne(om => om.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(om => om.UserId);

        modelBuilder.Entity<OrganizationMembership>()
            .HasOne(om => om.Organization)
            .WithMany(o => o.Memberships)
            .HasForeignKey(om => om.OrganizationId);
    }

    private void ConfigureIoTEntities(ModelBuilder modelBuilder)
    {
        // Configure table names
        modelBuilder.Entity<Asset>().ToTable("assets");
        modelBuilder.Entity<Sensor>().ToTable("sensors");
        modelBuilder.Entity<Reading>().ToTable("readings");
        modelBuilder.Entity<Alert>().ToTable("alerts");
        modelBuilder.Entity<Command>().ToTable("commands");
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // User indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();

        // Organization indexes
        modelBuilder.Entity<Organization>()
            .HasIndex(o => o.OwnerId);

        // IoT performance indexes
        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.OrganizationId);

        modelBuilder.Entity<Sensor>()
            .HasIndex(s => new { s.OrganizationId, s.AssetId });
        modelBuilder.Entity<Sensor>()
            .HasIndex(s => new { s.OrganizationId, s.IsActive });

        // Critical reading indexes for TimescaleDB
        modelBuilder.Entity<Reading>()
            .HasIndex(r => new { r.OrganizationId, r.SensorId, r.Timestamp })
            .HasDatabaseName("IX_Readings_OrgId_SensorId_Timestamp");

        modelBuilder.Entity<Reading>()
            .HasIndex(r => new { r.OrganizationId, r.Timestamp })
            .HasDatabaseName("IX_Readings_OrgId_Timestamp");

        // Alert indexes
        modelBuilder.Entity<Alert>()
            .HasIndex(a => new { a.OrganizationId, a.Acknowledged, a.Timestamp });

        // Command indexes
        modelBuilder.Entity<Command>()
            .HasIndex(c => new { c.OrganizationId, c.Status, c.CreatedAt });
        modelBuilder.Entity<Command>()
            .HasIndex(c => new { c.OrganizationId, c.UserId });
    }

    private void ConfigureTimescaleDb(ModelBuilder modelBuilder)
    {
        // Configure readings table for TimescaleDB hypertable
        modelBuilder.Entity<Reading>(entity =>
        {
            // Composite key with SensorId and Timestamp for TimescaleDB
            entity.HasKey(e => new { e.SensorId, e.Timestamp });
            
            // Fix timestamp column type to timestamptz
            entity.Property(e => e.Timestamp)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("NOW()");
                
            // Optimize for time-series queries
            entity.Property(e => e.Value)
                .HasColumnType("double precision");
                
            // Add organization_id for space partitioning
            entity.Property(e => e.OrganizationId)
                .HasMaxLength(50)
                .IsRequired();
        });

        // Configure other timestamp columns
        modelBuilder.Entity<Alert>()
            .Property(e => e.Timestamp)
            .HasColumnType("timestamptz");

        modelBuilder.Entity<Alert>()
            .Property(e => e.AcknowledgedAt)
            .HasColumnType("timestamptz");
    }
}
