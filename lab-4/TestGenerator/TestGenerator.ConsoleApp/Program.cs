using TestGenerator.Core;

namespace TestGenerator.ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Test Generator CLI ===");

        string sourceDir = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "TestsToGenerate");
        string outputDir = args.Length > 1 ? args[1] : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\MyProject.Tests\\Generated"));

        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        if (!Directory.Exists(sourceDir))
        {
            Console.WriteLine($"Error: Source directory not found: {sourceDir}");
            return;
        }

        var sourceFiles = Directory.GetFiles(sourceDir, "*.cs").ToList();

        if (sourceFiles.Count == 0)
        {
            Console.WriteLine("No .cs files found to process.");
            return;
        }

        Console.WriteLine($"Found {sourceFiles.Count} files. Starting generation...");

        int maxLoad = 3;
        int maxGen = 4;
        int maxWrite = 3;

        var pipeline = new Pipeline();

        try
        {
            await pipeline.GenerateAsync(sourceFiles, outputDir, maxLoad, maxGen, maxWrite);

            Console.WriteLine("\n[SUCCESS] Generation completed!");
            Console.WriteLine($"Results saved to: {outputDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Pipeline failed: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}