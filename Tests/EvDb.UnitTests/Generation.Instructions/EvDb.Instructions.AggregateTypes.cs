using EvDb.Core;
using EvDb.Scenes;
using EvDb.UnitTests;

[assembly: EvDbAggregateType<ICollection<StudentScore>, IEducationEventTypes>(
    "top-student")]
