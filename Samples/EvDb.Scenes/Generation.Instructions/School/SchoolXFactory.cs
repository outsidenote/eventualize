using EvDb.Core;
using System.Text.Json;


namespace EvDb.UnitTests;

[EvDbFactory<IStudentFlowEventTypes>]
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

    #region FoldingsFactories

    protected override Func<JsonSerializerOptions?, IEvDbFoldingUnit>[] FoldingsFactories { get; } =
        {
             StudentStatsFolding.Create
        };

    #endregion // FoldingsFactories
}