using EvDb.Core;

namespace EvDb.Scenes;

//-------------- events -------------
[EvDbPayload("StudentEnlistedEvent")]
public partial record StudentEnlistedEvent(StudentEntity Student);



