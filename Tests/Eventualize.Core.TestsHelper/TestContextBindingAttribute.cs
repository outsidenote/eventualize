using System.Reflection;
using Xunit.Sdk;

namespace Eventualize.Core.Tests;

public class TakeScreenshotAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodInfo)
    {
        TestContext.TestName = methodInfo.Name;
    }

    public override void After(MethodInfo methodInfo)
    {
    }
}
