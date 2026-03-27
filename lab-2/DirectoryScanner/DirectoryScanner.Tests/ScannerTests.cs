using DirectoryScanner.Core.Models;
using DirectoryScanner.Core.Services;
using Xunit;
using System.IO;

namespace DirectoryScanner.Tests;

public class ScannerTests : IDisposable
{
    private readonly string _tempTestPath;

    public ScannerTests()
    {
        _tempTestPath = Path.Combine(Path.GetTempPath(), "ScannerTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempTestPath);

        File.WriteAllBytes(Path.Combine(_tempTestPath, "root.bin"), new byte[100]);
        var sub = Directory.CreateDirectory(Path.Combine(_tempTestPath, "SubDir"));
        File.WriteAllBytes(Path.Combine(sub.FullName, "sub.bin"), new byte[50]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempTestPath))
            Directory.Delete(_tempTestPath, true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(100)]
    public async Task MaxWorkers_ShouldAllProduceCorrectTotalSize(int workers)
    {
        var engine = new ScannerEngine(workers);
        var cts = new CancellationTokenSource();
        var result = await engine.ScanAsync(_tempTestPath, cts.Token);

        Assert.Equal(150, result.Size);
        Assert.Equal(2, result.Children.Count);
    }

    [Fact]
    public async Task Cancel_ShouldStopExecutionAndReturnPartialData()
    {

        var engine = new ScannerEngine(2);
        var cts = new CancellationTokenSource();
        var task = engine.ScanAsync(_tempTestPath, cts.Token);
        cts.Cancel(); 

        var result = await task;
        Assert.NotNull(result);

    }

    [Fact]
    public async Task FolderAggregation_ShouldIgnoreReparsePoints()
    {
        string realDir = Path.Combine(_tempTestPath, "Real");
        Directory.CreateDirectory(realDir);
        File.WriteAllBytes(Path.Combine(realDir, "file.bin"), new byte[100]);

        string linkDir = Path.Combine(_tempTestPath, "LinkToReal");
        Directory.CreateSymbolicLink(linkDir, realDir);

        var engine = new ScannerEngine(2);
        var result = await engine.ScanAsync(_tempTestPath, CancellationToken.None);

        Assert.Equal(250, result.Size);
    }

    [Fact]
    public async Task Percentages_ShouldBeCalculatedCorrectly()
    {
        var engine = new ScannerEngine(4);
        var cts = new CancellationTokenSource();

        var result = await engine.ScanAsync(_tempTestPath, cts.Token);
        var subDirNode = result.Children.OfType<DirectoryNode>().First();
        var rootFileNode = result.Children.OfType<FileNode>().First();

        Assert.InRange(subDirNode.Percentage, 33.3, 33.4);
        Assert.InRange(rootFileNode.Percentage, 66.6, 66.7);
    }

    [Fact]
    public async Task ConcurrentAccess_ShouldNotCorruptData()
    {
        for (int i = 0; i < 50; i++)
        {
            File.WriteAllText(Path.Combine(_tempTestPath, $"file_{i}.txt"), "test");
        }

        var engine = new ScannerEngine(20); 
        var cts = new CancellationTokenSource();

        var result = await engine.ScanAsync(_tempTestPath, cts.Token);
        Assert.Equal(52, result.Children.Count);
    }

    [Fact]
    public async Task ScanAsync_ShouldIgnoreSymbolicLinks_ToAvoidInfiniteRecursionAndDoubleCounting()
    {
        string realDirPath = Path.Combine(_tempTestPath, "RealDir");
        Directory.CreateDirectory(realDirPath);
        File.WriteAllBytes(Path.Combine(realDirPath, "real_file.bin"), new byte[100]);

        string symLinkPath = Path.Combine(_tempTestPath, "SymLinkToRealDir");
        Directory.CreateSymbolicLink(symLinkPath, realDirPath);

        var engine = new ScannerEngine(1);
        var cts = new CancellationTokenSource();
        var result = await engine.ScanAsync(_tempTestPath, cts.Token);

        var symLinkNode = result.Children.FirstOrDefault(c => c.Name == "SymLinkToRealDir");

        Assert.Equal(250, result.Size);
        if (symLinkNode is DirectoryNode dirNode)
        {
            Assert.Empty(dirNode.Children);
        }
    }
}