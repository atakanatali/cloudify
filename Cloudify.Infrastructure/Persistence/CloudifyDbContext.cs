using Cloudify.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cloudify.Infrastructure.Persistence;

/// <summary>
/// Provides the Entity Framework Core database context for Cloudify persistence.
/// </summary>
public sealed class CloudifyDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudifyDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for configuring the context.</param>
    public CloudifyDbContext(DbContextOptions<CloudifyDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the resource group records.
    /// </summary>
    public DbSet<ResourceGroupRecord> ResourceGroups => Set<ResourceGroupRecord>();

    /// <summary>
    /// Gets the resource group tag records.
    /// </summary>
    public DbSet<ResourceGroupTagRecord> ResourceGroupTags => Set<ResourceGroupTagRecord>();

    /// <summary>
    /// Gets the environment records.
    /// </summary>
    public DbSet<EnvironmentRecord> Environments => Set<EnvironmentRecord>();

    /// <summary>
    /// Gets the resource records.
    /// </summary>
    public DbSet<ResourceRecord> Resources => Set<ResourceRecord>();

    /// <summary>
    /// Gets the capacity profile records.
    /// </summary>
    public DbSet<CapacityProfileRecord> CapacityProfiles => Set<CapacityProfileRecord>();

    /// <summary>
    /// Gets the storage profile records.
    /// </summary>
    public DbSet<StorageProfileRecord> StorageProfiles => Set<StorageProfileRecord>();

    /// <summary>
    /// Gets the credential profile records.
    /// </summary>
    public DbSet<CredentialProfileRecord> CredentialProfiles => Set<CredentialProfileRecord>();

    /// <summary>
    /// Gets the resource port policy records.
    /// </summary>
    public DbSet<ResourcePortPolicyRecord> ResourcePortPolicies => Set<ResourcePortPolicyRecord>();

    /// <summary>
    /// Gets the resource port allocation records.
    /// </summary>
    public DbSet<ResourcePortRecord> ResourcePorts => Set<ResourcePortRecord>();

    /// <summary>
    /// Gets the schema version records.
    /// </summary>
    public DbSet<SchemaVersionRecord> SchemaVersions => Set<SchemaVersionRecord>();

    /// <summary>
    /// Configures the EF Core model mappings for Cloudify persistence.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResourceGroupRecord>(entity =>
        {
            entity.ToTable("ResourceGroups");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Name).HasMaxLength(200).IsRequired();
            entity.Property(record => record.CreatedAt).IsRequired();
            entity.HasMany(record => record.Tags)
                .WithOne(tag => tag.ResourceGroup)
                .HasForeignKey(tag => tag.ResourceGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(record => record.Environments)
                .WithOne(environment => environment.ResourceGroup)
                .HasForeignKey(environment => environment.ResourceGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResourceGroupTagRecord>(entity =>
        {
            entity.ToTable("ResourceGroupTags");
            entity.HasKey(tag => new { tag.ResourceGroupId, tag.Key });
            entity.Property(tag => tag.Key).HasMaxLength(200).IsRequired();
            entity.Property(tag => tag.Value).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<EnvironmentRecord>(entity =>
        {
            entity.ToTable("Environments");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Name).HasConversion<int>().IsRequired();
            entity.Property(record => record.NetworkMode).HasConversion<int>().IsRequired();
            entity.Property(record => record.BaseDomain).HasMaxLength(255);
            entity.Property(record => record.CreatedAt).IsRequired();
            entity.HasIndex(record => new { record.ResourceGroupId, record.Name }).IsUnique();
        });

        modelBuilder.Entity<ResourceRecord>(entity =>
        {
            entity.ToTable("Resources");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Name).HasMaxLength(200).IsRequired();
            entity.Property(record => record.ResourceType).HasConversion<int>().IsRequired();
            entity.Property(record => record.State).HasConversion<int>().IsRequired();
            entity.Property(record => record.CreatedAt).IsRequired();
            entity.Property(record => record.AppImage).HasMaxLength(500);
            entity.HasIndex(record => new { record.EnvironmentId, record.Name }).IsUnique();
            entity.HasOne(record => record.Environment)
                .WithMany(environment => environment.Resources)
                .HasForeignKey(record => record.EnvironmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(record => record.PortPolicies)
                .WithOne(policy => policy.Resource)
                .HasForeignKey(policy => policy.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(record => record.AllocatedPorts)
                .WithOne(port => port.Resource)
                .HasForeignKey(port => port.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(record => record.CredentialProfile)
                .WithOne(profile => profile.Resource)
                .HasForeignKey<CredentialProfileRecord>(profile => profile.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasDiscriminator(record => record.ResourceType)
                .HasValue<RedisResourceRecord>(Cloudify.Domain.Models.ResourceType.Redis)
                .HasValue<PostgresResourceRecord>(Cloudify.Domain.Models.ResourceType.Postgres)
                .HasValue<MongoResourceRecord>(Cloudify.Domain.Models.ResourceType.Mongo)
                .HasValue<RabbitResourceRecord>(Cloudify.Domain.Models.ResourceType.Rabbit)
                .HasValue<AppServiceResourceRecord>(Cloudify.Domain.Models.ResourceType.AppService);
        });

        modelBuilder.Entity<CapacityProfileRecord>(entity =>
        {
            entity.ToTable("CapacityProfiles");
            entity.HasKey(record => record.ResourceId);
            entity.Property(record => record.Replicas).IsRequired();
            entity.Property(record => record.Notes).HasMaxLength(500);
            entity.HasOne(record => record.Resource)
                .WithOne(resource => resource.CapacityProfile)
                .HasForeignKey<CapacityProfileRecord>(record => record.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StorageProfileRecord>(entity =>
        {
            entity.ToTable("StorageProfiles");
            entity.HasKey(record => record.ResourceId);
            entity.Property(record => record.VolumeName).HasMaxLength(200).IsRequired();
            entity.Property(record => record.MountPath).HasMaxLength(400).IsRequired();
            entity.Property(record => record.SizeGb).IsRequired();
            entity.HasOne(record => record.Resource)
                .WithOne(resource => resource.StorageProfile)
                .HasForeignKey<StorageProfileRecord>(record => record.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CredentialProfileRecord>(entity =>
        {
            entity.ToTable("CredentialProfiles");
            entity.HasKey(record => record.ResourceId);
            entity.Property(record => record.Username).HasMaxLength(200).IsRequired();
            entity.Property(record => record.Password).HasMaxLength(500).IsRequired();
            entity.HasOne(record => record.Resource)
                .WithOne(resource => resource.CredentialProfile)
                .HasForeignKey<CredentialProfileRecord>(record => record.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResourcePortPolicyRecord>(entity =>
        {
            entity.ToTable("ResourcePortPolicies");
            entity.HasKey(policy => new { policy.ResourceId, policy.Port });
            entity.Property(policy => policy.Port).IsRequired();
        });

        modelBuilder.Entity<ResourcePortRecord>(entity =>
        {
            entity.ToTable("ResourcePorts");
            entity.HasKey(port => new { port.EnvironmentId, port.ResourceId, port.Port });
            entity.Property(port => port.Port).IsRequired();
            entity.HasIndex(port => new { port.EnvironmentId, port.Port }).IsUnique();
            entity.HasOne(port => port.Environment)
                .WithMany(environment => environment.AllocatedPorts)
                .HasForeignKey(port => port.EnvironmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SchemaVersionRecord>(entity =>
        {
            entity.ToTable("SchemaVersions");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Version).IsRequired();
            entity.Property(record => record.AppliedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
