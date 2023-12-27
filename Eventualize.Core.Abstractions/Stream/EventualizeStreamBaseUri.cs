using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core
{
    public record EventualizeStreamBaseUri(string Domain, string StreamType)
    {
    }
}