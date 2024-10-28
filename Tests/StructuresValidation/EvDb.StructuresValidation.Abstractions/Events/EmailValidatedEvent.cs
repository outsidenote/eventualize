using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.StructuresValidation.Abstractions.Events;

[EvDbDefinePayload("email-validated")]
public readonly partial record struct EmailValidatedEvent(string Email, bool IsValid);


