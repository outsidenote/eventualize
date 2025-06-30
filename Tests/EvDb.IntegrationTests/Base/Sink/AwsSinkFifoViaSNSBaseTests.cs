// Ignore Spelling: Sql Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;


public abstract class AwsSinkFifoViaSNSBaseTests : AwsSinkCommonBaseTests
{
    private static readonly string TOPIC_NAME = $"SNS_TEST_{Guid.NewGuid():N}.fifo";
    private static readonly string QUEUE_NAME = $"SQS_TEST_{Guid.NewGuid():N}.fifo";

    #region Ctor

    protected AwsSinkFifoViaSNSBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, Sinks.SQSMessageFormat.Raw, TOPIC_NAME, QUEUE_NAME)
    {
    }

    #endregion //  Ctor
}