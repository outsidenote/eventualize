using System.Collections.Immutable;
using System.Text.Json;
using EvDb.Core;
using EvDb.Scenes;
namespace EvDb.UnitTests;
using EvDb.Core.Internals;
using static EvDb.UnitTests.EvDbSchoolStreamTableMatching;

public sealed class EvDbSchoolStreamTopicsContext1 : EvDbTopicContextBase
{
    public EvDbSchoolStreamTopicsContext1(
        EvDbStream evDbStream,
        IEvDbEventMeta relatedEventMeta)
                : base(evDbStream, relatedEventMeta)
    {
    }

    public void Add(EvDb.Scenes.AvgMessage payload)
    {
        var tableNames = ToTables(EvDbTopic.DEFAULT_TOPIC);

        foreach (var tableName in tableNames)
        {
            base.Add(payload, EvDbTopic.DEFAULT_TOPIC, tableName); 
        }
    }

    public void Add(EvDb.Scenes.StudentPassedMessage payload)
    {
        var tableNames = ToTables(EvDbTopic.DEFAULT_TOPIC);

        foreach (var tableName in tableNames)
        {
            base.Add(payload, EvDbTopic.DEFAULT_TOPIC, tableName);
        }
    }


    public void Add(EvDb.Scenes.StudentFailedMessage payload)
    {
        var tableNames = ToTables("topic-1");
        foreach (var tableName in tableNames)
        {
            base.Add(payload, "topic-1", tableName); 
        }
    }


    public void Add(EvDb.Scenes.StudentPassedMessage payload, TopicsOfStudentPassedMessage topic)
    {
        string topicText = topic switch
        {
            TopicsOfStudentPassedMessage.Topic3 => "topic-3",
            TopicsOfStudentPassedMessage.Topic2 => "topic-2",
            TopicsOfStudentPassedMessage.Topic1 => "topic-1"
        };

        var tableNames = ToTables(topicText);

        foreach (var tableName in tableNames)
        {
            base.Add(payload, topicText, tableName);
        }
    }

}

internal static class EvDbSchoolStreamTableMatching
{
    public static string[] ToTables(string topic)
    {
        string[] tables = topic switch
        {

            "topic-3" => ["table1"],
            "topic-2" => ["table1", "table2"],
            EvDbTopic.DEFAULT_TOPIC => ["ev-db-topic"],
            _ => ["ev-db-topic"]
        };

        return tables;
    }
}