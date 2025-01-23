using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.IntegrationTests.EF.States;
public readonly record struct Person(int Id,
                                     string Name,
                                     DateOnly Birthday,
                                     Address? Address = null)
{
    public Email[] Emails { get; init; } = Array.Empty<Email>();
}
