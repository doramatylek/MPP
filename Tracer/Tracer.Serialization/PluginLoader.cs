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
            if (!Directory.Exists(pluginsDirectory))
            {
                Console.WriteLine($"Directory not found: {pluginsDirectory}");
                return 0;
            }

            var dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll");
            int totalLoaded = 0;

            foreach (var dll in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(ITraceResultSerializer).IsAssignableFrom(t)
                                    && t.IsClass && !t.IsAbstract);

                    foreach (var type in pluginTypes)
                    {
                        var plugin = Activator.CreateInstance(type) as ITraceResultSerializer;
                        if (plugin != null)
                        {
                            loadedPlugins.Add(plugin);
                            totalLoaded++;
                            Console.WriteLine($"Loaded plugin: {plugin.Format}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load {Path.GetFileName(dll)}: {ex.Message}");
                }
            }

            return totalLoaded;
        }

        public void SaveResults(TraceResult traceResult, string outputDirectory = null)
        {
            if (loadedPlugins.Count == 0)
            {
                Console.WriteLine("No plugins loaded. Nothing to save.");
                return;
            }

            outputDirectory ??= Directory.GetCurrentDirectory();
            Directory.CreateDirectory(outputDirectory);

            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    string fileName = $"trace_result.{plugin.Format}";
                    string filePath = Path.Combine(outputDirectory, fileName);

                    using var fs = File.Create(filePath);
                    plugin.Serialize(traceResult, fs);

                    Console.WriteLine($"Saved {fileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save {plugin.Format}: {ex.Message}");
                }
            }
        }
        public IEnumerable<ITraceResultSerializer> GetPlugins() => loadedPlugins;
    }
}
