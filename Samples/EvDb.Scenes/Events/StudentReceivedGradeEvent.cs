﻿using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("student-received-grade")]
public partial record StudentReceivedGradeEvent(int TestId, int StudentId, double Grade, string? Comments = null);


