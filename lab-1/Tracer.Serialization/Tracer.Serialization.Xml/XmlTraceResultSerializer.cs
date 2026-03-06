using Newtonsoft.Json;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Xml
{
    public class XmlTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "xml";

        public void Serialize(TraceResult traceResult, Stream output)
        {

            string json = JsonConvert.SerializeObject(traceResult, Newtonsoft.Json.Formatting.Indented);
           
            var xmlDoc = JsonConvert.DeserializeXNode(json, "TraceResult");

            using (var writer = new StreamWriter(output))
            {
                writer.Write(xmlDoc.ToString());
            }
        }
    }
}
