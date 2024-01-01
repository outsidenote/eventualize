//namespace EvDb.UnitTests;

//using EvDb.Core;
//using EvDb.Scenes;

//public class ApiFirst
//{
//    [Fact]
//    public void ApiDesign()
//    {
//        var builder = EvDbBuilder
//            //.AddEventType<CourseCreated>()
//            //.AddEventType<StudentAppliedToCourse>()
//            //.AddStream<IEducationStream>()
//            .AddStreamType(EvDbStreamType type)
//            .AddEventTypes<IEducationStream>()// where T: IEvDbEventTyppes
//            .AddEntityId("id:123")
//            .AddAggregate<TStateA>()
//            .AddEntityId("id:123")
//            .Build();

//        var builder = EvDbBuilder
//            //.AddEventType<CourseCreated>()
//            //.AddEventType<StudentAppliedToCourse>()
//            //.AddStream<IEducationStream>()
//            .Create<IEducationStream, TStateA>(EvDbStreamUri uri);

//        var aggA = builder.AddAggregate<TStateA>()
//            .build();
//        var aggB = builder.AddAggregate<TStateB>()
//            .build();

//        aggA.Events.Async(payload);
//    }

//}

//public interface IEvDbBuilder
//{
//    IEvDbBuilder AddStream(EvDbStreamId uri);
//}

//public static class EvDbBuilder
//{
//    EvDbBuilder AddStream(EvDbStreamId uri) => throw new NotImplementedException();
//}

//public partial interface IEvDbEventTyppes
//{ 
//}

//[EvDbStream("Domain", "EntityType")]
//public partial interface IEducationStream<CourseCreated, ScheduleTest, StudentAppliedToCourse, StudentCourseApplicationDenied>: IEvDbEventTyppes
//{ 
//}

//public interface IEducationStream: IEvDbEventStream
//{
//    //Task SendAsync<T>(T item);

//    Task SendAsync(CourseCreated courseCreated);
//    Task SendAsync(ScheduleTest scheduleTest);
//    Task SendAsync(StudentAppliedToCourse applied);
//    Task SendAsync(StudentCourseApplicationDenied applicationDenyed);
//    Task SendAsync(StudentEnlisted enlisted);
//    Task SendAsync(StudentQuitCourse quit);
//    Task SendAsync(StudentReceivedGrade receivedGrade);
//    Task SendAsync(StudentRegisteredToCourse registeredToCourse);
//    Task SendAsync(StudentTestSubmitted testSubmitted);
//}
