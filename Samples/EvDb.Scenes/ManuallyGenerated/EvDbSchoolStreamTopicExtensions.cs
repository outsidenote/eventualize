//#nullable enable
//#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
//#pragma warning disable CS0105 // Using directive appeared previously in this namespace
//#pragma warning disable CS0108 // hides inherited member.

//using System.Collections.Immutable;
//using System.Text.Json;
//// ####################  GENERATED AT: 2024-10-16 08:11:46 ####################
//using EvDb.Core;
//using EvDb.Scenes;
//namespace EvDb.UnitTests;
//using EvDb.Core.Internals;
////using Microsoft.Extensions.DependencyInjection;
////using EvDb.UnitTests;

//public static class EvDbSchoolStreamTopicExtensions
//{

//    public static EvDbSchoolStreamFactorySnapshotEntry AddTopics(this EvDbSchoolStreamFactorySnapshotEntry context, Action<EvDbSchoolStreamTopicDefinition> createTopicGroup)
//    {
//        return context;
//    }

//    public class EvDbSchoolStreamTopicDefinitionContext : EvDbSchoolStreamTopicDefinition
//    {
//        static internal readonly EvDbSchoolStreamTopicDefinitionContext Instance = new();

//        public EvDbSchoolStreamTopicDefinition WithTransformation(Func<byte[], byte[]> transform)
//        {
//            return this;
//        }

//        public EvDbSchoolStreamTopicDefinition WithTransformation<T>() where T : IEvDbTopicTransformer
//        {
//            return this;
//        }
//    }

//    public class EvDbSchoolStreamTopicDefinition
//    {
//        public EvDbSchoolStreamTopicDefinitionContext CreateTopicGroup(string groupName, EvDbSchoolStreamTopicOptions topics, params EvDbSchoolStreamTopicOptions[] additionalTopics)
//        {
//            return EvDbSchoolStreamTopicDefinitionContext.Instance;
//        }
//    }
//}
