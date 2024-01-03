using EvDb.Core;

namespace EvDb.Scenes;

//-------------- events -------------
public record StudentEnlisted(StudentEntity Student) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType => "StudentEnlisted"; // BAD PRACTICE is nameof(StudentEnlisted);
}



