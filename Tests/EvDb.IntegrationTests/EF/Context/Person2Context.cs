using Microsoft.EntityFrameworkCore;

namespace EvDb.IntegrationTests.EF;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class Person2Context : DbContext
{
    public DbSet<PersonEntity> Persons { get; set; }

    public DbSet<EmailEntity> Emails { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Person2Context(DbContextOptions<PersonContext> options) : base(options)
    {
        // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(m => Trace.WriteLine(m), LogLevel.Information)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonEntity>(ConfigurePerson);
        modelBuilder.Entity<EmailEntity>(ConfigureEmail);
    }

    private void ConfigureEmail(EntityTypeBuilder<EmailEntity> builder)
    {
        builder.ToTable("Emails2");

        // Set the primary key
        builder.HasKey(e => e.Id);

        // Configure properties
        builder.Property(e => e.Id)
            .IsRequired(); // Optional: Set max length

        builder.Property(e => e.Value)
            .IsRequired() // Mark as required
            .HasMaxLength(255); // Optional: Set max length

        builder.Property(e => e.Domain)
            .IsRequired() // Mark as required
            .HasMaxLength(255); // Optional: Set max length

        builder.Property(e => e.Category)
            .IsRequired() // Mark as required
            .HasMaxLength(255); // Optional: Set max length

        // Configure the foreign key relationship
        builder.HasOne<PersonEntity>() // Define the relationship with PersonEntity
            .WithMany(p => p.Emails) // PersonEntity has many Emails
            .HasForeignKey(e => e.PersonId) // Define the foreign key property
            .IsRequired() // The foreign key must have a value
            .OnDelete(DeleteBehavior.Cascade); // Delete emails when the related person is deleted

    }

    private void ConfigurePerson(EntityTypeBuilder<PersonEntity> builder)
    {
        builder.ToTable("People2");
        builder.HasKey(p => p.Id);

        // Configure Birthday
        builder.Property(p => p.Birthday)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>()
        .IsRequired();

        // Configure Emails (one-to-many relationship)
        builder.HasMany(p => p.Emails)
            .WithOne()
            .HasForeignKey(
            e => e.PersonId)
            //.IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Address
        builder.OwnsOne(p => p.Address, a =>
        {
            a.Property(ad => ad.Country).HasColumnName("Country");
            a.Property(ad => ad.City).HasColumnName("City");
            a.Property(ad => ad.Street).HasColumnName("Street");
        });
    }
}
