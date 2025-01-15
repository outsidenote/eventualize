using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.IntegrationTests.EF.States;
public readonly record struct Person(int Id,
                                     string Name,
                                     DateOnly Birthday,
                                     Email[] Emails,
                                     Address Address)
{
}
