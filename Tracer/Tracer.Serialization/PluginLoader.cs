using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization
{
    public class PluginLoader
    {
        private readonly List<ITraceResultSerializer> loadedPlugins = new();

        public int LoadPlugins(string pluginsDirectory)
        {
            if (string.IsNullOrEmpty(pluginsDirectory))
                throw new ArgumentException("Plugins directory cannot be null or empty", nameof(pluginsDirectory));

            if (!Directory.Exists(pluginsDirectory))
                throw new DirectoryNotFoundException($"Directory not found: {pluginsDirectory}");

            int totalLoaded = 0;

            foreach (var dll in Directory.GetFiles(pluginsDirectory, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);

                    foreach (var type in assembly.GetTypes()
                        .Where(t => typeof(ITraceResultSerializer).IsAssignableFrom(t)
                                 && t.IsClass && !t.IsAbstract))
                    {
                        if (Activator.CreateInstance(type) is ITraceResultSerializer plugin)
                        {
                            loadedPlugins.Add(plugin);
                            totalLoaded++;
                        }
                    }
                }
                catch (Exception ex) when (ex is BadImageFormatException or FileLoadException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load plugin from {Path.GetFileName(dll)}", ex);
                }
            }

            return totalLoaded;
        }

        public void SaveResults(TraceResult traceResult, string outputDirectory = null)
        {
            if (traceResult == null)
                throw new ArgumentNullException(nameof(traceResult));

            if (loadedPlugins.Count == 0)
                throw new InvalidOperationException("No plugins loaded. Call LoadPlugins() first.");

            outputDirectory ??= Directory.GetCurrentDirectory();

            Directory.CreateDirectory(outputDirectory);

            foreach (var plugin in loadedPlugins)
            {
                string filePath = Path.Combine(outputDirectory, $"trace_result.{plugin.Format}");

                try
                {
                    using var fs = File.Create(filePath);
                    plugin.Serialize(traceResult, fs);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to save {plugin.Format} format to {filePath}", ex);
                }
            }
        }

        public IEnumerable<ITraceResultSerializer> GetPlugins() => loadedPlugins;
    }
}