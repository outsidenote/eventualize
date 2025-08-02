using EvDb.Core;
using System.Collections.Immutable;
using System.Text.Json;

namespace EvDb.UnitTests;

[Trait("Kind", "UnitTest")]
public class NamedTypesTests
{  
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
