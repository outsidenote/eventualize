﻿using EvDb.Core;
using EvDb.Scenes;
using System.Text.Json;


namespace EvDb.UnitTests;

[EvDbStreamFactory<ISchoolCollection>]
//[EvDbStreamFactory<ISchoolCollection>(CollectionName = "School")]
public partial class SchoolXFactory
{
    #region Ctor

    public SchoolXFactory(IEvDbStorageAdapter storageAdapter) : base(storageAdapter)
    {
    }

    #endregion // Ctor

    public override string Kind { get; } = "student-avg";

    #region JsonSerializerOptions

    public override JsonSerializerOptions? JsonSerializerOptions { get; } = StudentFlowEventTypesContext.Default.Options;

    #endregion // JsonSerializerOptions

    #region Partition

    public override EvDbPartitionAddress Partition { get; } = new EvDbPartitionAddress("school-records", "students");

    #endregion // Partition

    #region ViewFactories

    protected override Func<JsonSerializerOptions?, IEvDbView>[] ViewFactories { get; } =
        {
             StudentStatsView.Create
        };

    #endregion // ViewFactories
}