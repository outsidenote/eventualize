using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Immutable;
using System.Text.Json;

namespace EvDb.UnitTests;

public partial class EvDbSchoolStreamPublicationFoldBase: IEvDbPublicationTypes
{
    private readonly EvDbSchoolStream _evDbStream;
    private readonly JsonSerializerOptions? _serializationOptions;

    public EvDbSchoolStreamPublicationFoldBase(EvDbSchoolStream evDbStream)
    {
        _evDbStream = evDbStream;
        _serializationOptions = _evDbStream.Options;
    }
    public void OnPublication(
        EvDbEvent e,
        IImmutableList<IEvDbViewStore> views)
    {
        EvDbSchoolStreamViews states = _evDbStream.Views;
        SchoolStreamPublicationProducer producer = new(_evDbStream, e);
        IEvDbEventConverter c = e;
        switch (e.EventType)
        {
            case "StudentEnlistedEvent":
                {
                    var payload = c.GetData<StudentEnlistedEvent>(_serializationOptions);
                    Publication(payload, states, e, producer);
                    break;
                }
            default:
                break;
        }
    }

    protected virtual void Publication(
        StudentEnlistedEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        SchoolStreamPublicationProducer producer)
    {
    }

    protected virtual void Publication(
        ScheduleTestEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        SchoolStreamPublicationProducer producer)
    {
    }
}
