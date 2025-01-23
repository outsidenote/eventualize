using EvDb.IntegrationTests.EF.States;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvDb.IntegrationTests.EF;

public record EmailEntity(string Value, string Domain, string Category)
{
    public int PersonId { get; init; }
}
