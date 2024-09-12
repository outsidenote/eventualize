using EvDb.Core;
using EvDb.Core.Internals;
using EvDb.Scenes;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EvDb.UnitTests;


public sealed class SchoolStreamPublicationProducer : PublicationProducerBase
{
    public SchoolStreamPublicationProducer(
        EvDbStream evDbStream,
        IEvDbEventMeta relatedEventMeta) : base(evDbStream, relatedEventMeta)
    {
    }

    public void Publish(CourseCreatedPublicEvent payload)
    {
        base.Publish(payload);
    }

    public void Publish(StudentQuitCoursePublicEvent payload)
    {
        base.Publish(payload);
    }
}
