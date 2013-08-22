using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Pools.Abstract
{
    public interface IPoolable
    {
        bool IsWorking();
        string ServiceHost { get; }
    }
}
