using EvDb.Core;
using System.Collections.Immutable;
using System.Text.Json;

namespace EvDb.UnitTests;

public class NamedTypesTests
{
    private const string json = """
                        {
                          "parent": "001",
                          "state": "recording"
                        }
                        """;

    #region EvDbTelemetryContextName_TryParse_Test

    [Fact]
    public void EvDbTelemetryContextName_Equality_Test()
    {
        EvDbTelemetryContextName context1 = EvDbTelemetryContextName.Parse(json);
        EvDbTelemetryContextName context2 = EvDbTelemetryContextName.Parse(json);
        Assert.Equal(context1, context2);
        Assert.True(context1.Equals(context1.ToArray()));
        Assert.True(context1.Equals(context1.ToImmutableArray()));
        Assert.True(context1.Equals(context1.AsSpan()));
    }

    [Fact]
    public void EvDbTelemetryContextName_Cast_Test()
    {
        EvDbTelemetryContextName context1 = EvDbTelemetryContextName.Parse(json);
        byte[] value1 = context1.ToArray();
        EvDbTelemetryContextName context2 = value1;
        byte[] value2 = context2;
        EvDbTelemetryContextName context3 = value2;
        ReadOnlySpan<byte> value4 = context3.AsSpan();
        EvDbTelemetryContextName context5 = EvDbTelemetryContextName.FromSpan(value4);

        Assert.Equal(context1, context5);
    }

    [Fact]
    public void EvDbTelemetryContextName_TryParse_Test()
    {

        EvDbTelemetryContextName context = EvDbTelemetryContextName.Parse(json);
        var json1 = context.ToString("i");
        Assert.Equal(json, json1);
    }

    #endregion //  EvDbTelemetryContextName_TryParse_Test

    #region EvDbTelemetryContextName_FromJsonDoc_Test

    [Fact]
    public void EvDbTelemetryContextName_FromJsonDoc_Test()
    {
        var doc = JsonDocument.Parse(json);
        var context = EvDbTelemetryContextName.FromJson(doc);
        var json1 = context.ToString("i");
        Assert.Equal(json, json1);
    }

    #endregion //  EvDbTelemetryContextName_FromJsonDoc_Test

    #region EvDbTelemetryContextName_AsMemory_Test

    [Fact]
    public void EvDbTelemetryContextName_AsMemory_Test()
    {
        EvDbTelemetryContextName context = EvDbTelemetryContextName.Parse(
                                                    json.AsMemory());
        var json1 = context.ToString("i");
        Assert.Equal(json, json1);
    }

    #endregion //  EvDbTelemetryContextName_AsMemory_Test

    #region EvDbTelemetryContextName_AsSpan_Test

    [Fact]
    public void EvDbTelemetryContextName_AsSpan_Test()
    {
        EvDbTelemetryContextName context = EvDbTelemetryContextName.Parse(
                                                    json.AsSpan());
        var json1 = context.ToString("i");
        Assert.Equal(json, json1);
    }

    #endregion //  EvDbTelemetryContextName_AsSpan_Test

    #region EvDbStreamTypeName_Validition_Test

    [Theory]
    [InlineData("Test", true)]
    [InlineData("Test123", true)]
    [InlineData("Test_123", true)]
    [InlineData("Test-123", true)]
    [InlineData("Test.123", true)]
    [InlineData("Test/123", true)]
    [InlineData("Test:123", true)]
    [InlineData(@"Test\123", false)]
    [InlineData("Test 123", false)]
    [InlineData("Test@123", false)]
    [InlineData("Test#123", false)]
    [InlineData("Test$123", false)]
    [InlineData("Test%123", false)]
    public void EvDbStreamTypeName_Validition_Test(string name, bool shouldSucceed)
    {
        bool isValid = EvDbStreamTypeName.TryFrom(name, out EvDbStreamTypeName root);
        Assert.Equal(isValid, shouldSucceed);
    }

    #endregion //  EvDbStreamTypeName_Validition_Test

    #region EvDbMessageId_Test

    [Fact]
    public void EvDbMessageId_Test()
    {
        var guid = Guid.NewGuid();
        EvDbMessageId id1 = guid;
        EvDbMessageId id2 = EvDbMessageId.From(guid);
        Assert.True(id1.Equals(guid));
        Assert.Equal(id1, id2);
        var jsonGuid = JsonSerializer.Serialize(guid);
        var jsonId = JsonSerializer.Serialize(id1);
        Assert.Equal(jsonGuid, jsonId);
    }

    #endregion //  EvDbMessageId_Test
}
