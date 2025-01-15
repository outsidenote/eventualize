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
using System.Runtime.ConstrainedExecution;

public class PersonContext : DbContext
{
    public DbSet<PersonEntity> Persons { get; set; }

    public PersonContext(DbContextOptions<PersonContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonEntity>(ConfigurePerson);
    }

    private void ConfigurePerson(EntityTypeBuilder<PersonEntity> builder)
    {
        builder.HasKey(p => p.Id);

        // Configure Birthday
        builder.Property(p => p.Birthday)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>()
            .IsRequired();

        // Configure Emails
        builder.Property(p => p.Emails)
            .HasConversion(
                v => string.Join(";", v.Select(e => e.ToString())), // Convert to string
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                      .Select(e => (Email)e).ToArray() // Convert back to Email[]
            );

        // Configure Address
        builder.OwnsOne(p => p.Address, a =>
        {
            a.Property(ad => ad.Country).IsRequired();
            a.Property(ad => ad.City).IsRequired();
            a.Property(ad => ad.Street).IsRequired();
        });
    }
}
