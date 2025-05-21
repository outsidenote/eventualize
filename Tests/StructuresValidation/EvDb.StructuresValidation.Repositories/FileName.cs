#nullable enable
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS0105 // Using directive appeared previously in this namespace
#pragma warning disable CS0108 // hides inherited member.

using System.Collections.Immutable;
using System.Text.Json;
// ####################  GENERATED AT: 2025-05-21 11:07:24 ####################
using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;
namespace EvDb.StructuresValidation.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable once UnusedType.Global

[System.CodeDom.Compiler.GeneratedCode("EvDb.SourceGenerator", "4.0.1.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

partial class CustomerEntityModelStreamFactory1 : EvDbStreamFactoryBase<IEvDbCustomerEntityModelStream>,
        IEvDbCustomerEntityModelStreamFactory
{
    #region Ctor

    public CustomerEntityModelStreamFactory1(
            [FromKeyedServices("CLM_CEM")] IEvDbStorageStreamAdapter storageAdapter,

            ILogger<EvDbCustomerEntityModelStream> logger,
            TimeProvider? timeProvider = null) : base(logger, storageAdapter, timeProvider ?? TimeProvider.System)
    {
        ViewFactories = new IEvDbViewFactory[]
        {
        };
    }

    #endregion // Ctor

    protected override IEvDbViewFactory[] ViewFactories { get; }

    #region RootAddress

    public override EvDbRootAddressName RootAddress { get; } = "CLM_CEM";

    #endregion // RootAddress

    #region OnCreate

    protected override EvDbCustomerEntityModelStream OnCreate(
            string streamId,
            IImmutableList<IEvDbViewStore> views,
            long lastStoredEventOffset)
    {
        EvDbCustomerEntityModelStream stream =
            new(
                _logger,
                this,
                views,
                _storageAdapter,
                streamId,
                lastStoredEventOffset);

        return stream;
    }

    #endregion // OnCreate                    
}
