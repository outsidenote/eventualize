using System;
using System.Collections.Generic;
using System.Text;

namespace EvDb.SourceGenerator.Helpers;
internal static class GeneralExtensions
{
    public static string GenSuffix(this string fileName) => $"{fileName}.generated.{Guid.NewGuid():N}.cs";
}
