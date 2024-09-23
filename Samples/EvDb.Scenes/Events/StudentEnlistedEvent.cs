using EvDb.Core;

namespace EvDb.Scenes;

//-------------- events -------------
[EvDbDefinePayload("StudentEnlistedEvent")]
public partial record StudentEnlistedEvent(StudentEntity Student);



