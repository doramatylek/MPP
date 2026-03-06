
using System.Text.Json;
using Tracer.Core;
using Tracer.Serialization.Abstractions;


namespace Tracer.Serialization.Json
{
    public class JsonTraceResultSerializer : ITraceResultSerializer
    {
        public string Format { get; } = "json";
        public void Serialize(TraceResult traceResult, Stream output)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true 
            };

            JsonSerializer.Serialize(output, traceResult, options);
        }

    }
}
