// Ignore Spelling: Sql Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;


public abstract class AwsSinkBaseTests : AwsSinkCommonBaseTests
{
    private static readonly string TOPIC_NAME = $"SNS_TEST_{Guid.NewGuid():N}";
    private static readonly string QUEUE_NAME = $"SQS_TEST_{Guid.NewGuid():N}";

    #region Ctor

    protected AwsSinkBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, Sinks.SQSMessageFormat.Raw, TOPIC_NAME, QUEUE_NAME)
    {
    }

    #endregion //  Ctor
}