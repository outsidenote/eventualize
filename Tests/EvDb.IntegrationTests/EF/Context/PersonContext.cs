using EvDb.Scenes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.IntegrationTests.EF;

using EvDb.IntegrationTests.EF.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;

public class PersonContext : DbContext
{
    public DbSet<PersonEntity> Persons { get; set; }

    public DbSet<EmailEntity> Emails { get; set; }

    public PersonContext(DbContextOptions<PersonContext> options) : base(options)
    {
        // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo( m => Trace.WriteLine(m), LogLevel.Information)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        modelBuilder.Entity<PersonEntity>(ConfigurePerson);
        modelBuilder.Entity<EmailEntity>(ConfigureEmail);
        modelBuilder.Entity<EmailEntity>(entity =>
        {
            entity.ToTable("Emails");
            entity.HasKey(e => e.Value); // Set Value as Primary Key
        });
    }

    private void ConfigureEmail(EntityTypeBuilder<EmailEntity> builder)
    {
        builder.ToTable("Emails");

        // Set the primary key
        builder.HasKey(e => e.Value);

        // Configure properties
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
        builder.ToTable("People");
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
