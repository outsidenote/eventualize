using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.UnitTests;

public class NamedTypesTests
{
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
    public void EvDbRootAddressName_Validition_Test(string name, bool shouldSucceed)
    {
        bool isValid = EvDbRootAddressName.TryFrom(name, out EvDbRootAddressName root);
        Assert.Equal(isValid, shouldSucceed);
    }
}
