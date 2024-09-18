// Ignore Spelling: OutboxHandler Outbox

using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;


[EvDbOutboxTypes<AvgOutbox>]
[EvDbOutboxTypes<StudentPassOutbox>]
[EvDbOutboxTypes<StudentFailOutbox>]
[EvDbOutbox<SchoolStreamFactory>]
public partial class EvDbSchoolStreamOutbox
{
    protected override void OutboxHandler(
        StudentReceivedGradeEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        EvDbSchoolStreamOutboxContext outbox)
    {
        var state = views.ALL;
        var avg = new AvgOutbox(state.Sum / (double)state.Count);
        outbox.Add(avg);
        var studentName = views.StudentStats.Students
            .First(m => m.StudentId == payload.StudentId)
            .StudentName;
        if (payload.Grade >= 60)
        {
            var pass = new StudentPassOutbox(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            outbox.Add(pass);
        }
        else
        { 
            var fail = new StudentFailOutbox(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            outbox.Add(fail);
        }
    }    
}

