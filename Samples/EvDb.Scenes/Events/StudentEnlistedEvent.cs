using EvDb.Core;

namespace EvDb.Scenes;

//-------------- events -------------
[EvDbEventPayload("StudentEnlistedEvent")]
public partial record StudentEnlistedEvent(StudentEntity Student);



