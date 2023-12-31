using Generator.Equals;

namespace EvDb.Core.Tests;

[Equatable]
public partial record TestEventDataType(string A, int B);