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

    public EvDbSchoolStreamTopicDefinitionContext CreateTopicGroup(string groupName, EvDbSchoolStreamTopics topics, params EvDbSchoolStreamTopics[] additionalTopics)
    {
        throw new NotImplementedException();
    }
}

public enum EvDbSchoolStreamTopics
{
    Default,
    Topic1,
    Topic2,
    Topic3
}