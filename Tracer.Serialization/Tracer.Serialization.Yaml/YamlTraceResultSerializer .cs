using System.Text;
using Tracer.Core;
using Tracer.Serialization.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tracer.Serialization.Yaml
{
    public class YamlTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "yaml";

        public void Serialize(TraceResult traceResult, Stream output)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            using var writer = new StreamWriter(output, Encoding.UTF8, leaveOpen: true);
            serializer.Serialize(writer, traceResult);
            writer.Flush();
        }
    }
}
