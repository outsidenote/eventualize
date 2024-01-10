using EvDb.Core;

namespace EvDb.Scenes;

//-------------- events -------------
public record StudentEnlistedEvent(StudentEntity Student) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType => "StudentEnlistedEvent"; // BAD PRACTICE is nameof(StudentEnlisted);
}



