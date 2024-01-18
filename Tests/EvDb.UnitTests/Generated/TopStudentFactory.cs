using EvDb.Core;
using System.CodeDom.Compiler;

namespace EvDb.UnitTests;

[GeneratedCode("from IEducationEventTypes<T0, T1,...>", "v0")]
partial class TopStudentFactory : TopStudentFactoryBase
{
    public TopStudentFactory(IEvDbStorageAdapter storageAdapter) : base(storageAdapter)
    {
    }
}
