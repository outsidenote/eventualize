﻿using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Internals;

public interface IEvDbRegistrationEntry
{
    IServiceCollection Services { get; }
}