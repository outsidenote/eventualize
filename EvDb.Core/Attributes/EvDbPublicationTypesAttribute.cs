﻿#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EvDbPublicationTypesAttribute<T> : Attribute
    where T : IEvDbEventPayload
{
}

