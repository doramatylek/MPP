using System.Threading.Tasks.Dataflow;

namespace TestGenerator.Core;

public class Pipeline
{
    public Task GenerateAsync(IEnumerable<string> sourceFiles, string outputFolder,
        int maxLoadThreads, int maxGenThreads, int maxWriteThreads)
    {
        Directory.CreateDirectory(outputFolder);

        var loadBlock = new TransformBlock<string, FileContent>(
            async path => new FileContent
            {
                FilePath = path,
                Code = await File.ReadAllTextAsync(path)
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxLoadThreads }
        );

        var generateBlock = new TransformManyBlock<FileContent, TestClassInfo>(
            content => CodeGenerator.GenerateTests(content.Code),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxGenThreads }
        );

        var writeBlock = new ActionBlock<TestClassInfo>(
            async testInfo =>
            {
                string path = Path.Combine(outputFolder, testInfo.FileName);
                await File.WriteAllTextAsync(path, testInfo.Code);
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxWriteThreads }
        );

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        loadBlock.LinkTo(generateBlock, linkOptions);
        generateBlock.LinkTo(writeBlock, linkOptions);

        foreach (var file in sourceFiles)
        {
            loadBlock.Post(file);
        }
        loadBlock.Complete();

        return writeBlock.Completion; 
    }
}