namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbEventTypesAttribute<T> : Attribute
    where T : IEvDbEventPayload
{
}



//override void PublicFold(TPayload payload, Views views, OutputSetter outs)
//{

//    outs.PublishQ1(new All(..));
//    outs.PublishQ2(new All(..));
//}