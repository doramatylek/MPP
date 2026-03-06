using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracer.Core;

namespace Tracer.Serialization.Abstractions
{
    public interface ITraceResultSerializer
    {
        public string Format { get; }
        public void Serialize(TraceResult traceResult, Stream to);
    }
}
