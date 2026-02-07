using System.Xml.Serialization;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Xml
{
    public class XmlTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "xml";

        public void Serialize(TraceResult traceResult, Stream output)
        {
            var serializer = new XmlSerializer(typeof(TraceResult));
            serializer.Serialize(output, traceResult);
        }
    }
}
