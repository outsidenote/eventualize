using EvDb.Core;

namespace EvDb.Scenes;

//-------------- events -------------
[EvDbDefineEventPayload("StudentEnlistedEvent")]
public partial record StudentEnlistedEvent(StudentEntity Student);



