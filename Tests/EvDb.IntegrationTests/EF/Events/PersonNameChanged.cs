using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbDefineEventPayload("name")]
public readonly partial record struct PersonNameChanged(int Id, string Name);

