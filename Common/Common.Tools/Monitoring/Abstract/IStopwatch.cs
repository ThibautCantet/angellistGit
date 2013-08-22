using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Monitoring.Interfaces
{
    public interface IStopwatch
    {
        long ElapsedNanoseconds { get; }
        void Start();
        void Stop();
    }
}
