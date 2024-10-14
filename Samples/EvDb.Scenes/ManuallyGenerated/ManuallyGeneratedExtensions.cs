using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection;

using EvDb.Core;
using EvDb.UnitTests;

public static class ManuallyGeneratedExtensions
{

    public static EvDbSchoolStreamFactorySnapshotEntry AddTopics(this EvDbSchoolStreamFactorySnapshotEntry context, Action<EvDbSchoolStreamTopicDefinition> createTopicGroup)
    {
        throw new NotImplementedException();
    }
}

public class EvDbSchoolStreamTopicDefinitionContext: EvDbSchoolStreamTopicDefinition
{
    public EvDbSchoolStreamTopicDefinition WithTransformation(Func<byte[], byte[]> transform)
    {
        throw new NotImplementedException();
    }

    public EvDbSchoolStreamTopicDefinition WithTransformation<T>() where T: IEvDbTopicTransformer
    {
        throw new NotImplementedException();
    }
}

public class EvDbSchoolStreamTopicDefinition
{

    public EvDbSchoolStreamTopicDefinitionContext CreateTopicGroup(string groupName, EvDbSchoolStreamTopics topics)
    {
        throw new NotImplementedException();
    }
}

[Flags]
public enum EvDbSchoolStreamTopics
{
    None = 0,
    Topic1 = 1,
    Topic2 = 2,
    Topic3 = 4,
    All = Topic1 | Topic2 | Topic3
}