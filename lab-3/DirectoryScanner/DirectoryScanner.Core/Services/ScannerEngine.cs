using System.Collections.Concurrent;
using DirectoryScanner.Core.Models;

namespace DirectoryScanner.Core.Services;

public class ScannerEngine
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<DirectoryNode> _queue = new();
    private readonly int _maxThreads;

    private int _pendingTasks = 0;
    private TaskCompletionSource _tcs = new();

    public ScannerEngine(int maxThreads)
    {
        _maxThreads = maxThreads;
        _semaphore = new SemaphoreSlim(maxThreads);
    }

    public async Task<DirectoryNode> ScanAsync(string path, CancellationToken token)
    {
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingTasks = 0;

        var root = new DirectoryNode
        {
            Name = string.IsNullOrEmpty(Path.GetFileName(path)) ? path : Path.GetFileName(path),
            FullPath = path
        };

        Interlocked.Increment(ref _pendingTasks);
        _queue.Enqueue(root);

        StartWorkers(token);

        try
        {
            await _tcs.Task.WaitAsync(token);
        }
        catch (OperationCanceledException) { }

        CalculateFinalSizes(root);
        return root;
    }

    private void StartWorkers(CancellationToken token)
    {
        for (int i = 0; i < _maxThreads; i++)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (!_queue.TryDequeue(out var node))
                    {
                        if (Interlocked.CompareExchange(ref _pendingTasks, 0, 0) == 0)
                            break;

                        await Task.Yield();
                        continue;
                    }

                    bool enteredSemaphore = false;
                    try
                    {
                        await _semaphore.WaitAsync(token);
                        enteredSemaphore = true;

                        ProcessDirectory(node, token);
                    }
                    catch (OperationCanceledException) { }
                    finally
                    {
                        if (enteredSemaphore)
                            _semaphore.Release();

                        if (Interlocked.Decrement(ref _pendingTasks) == 0)
                        {
                            _tcs.TrySetResult();
                        }
                    }
                }
            }, token);
        }
    }

    private void ProcessDirectory(DirectoryNode node, CancellationToken token)
    {
        try
        {
            var di = new DirectoryInfo(node.FullPath);
            if (IsSymlink(di)) return;

            foreach (var file in di.GetFiles())
            {
                if (token.IsCancellationRequested) break;
                if (IsSymlink(file)) continue;

                node.Children.Add(new FileNode
                {
                    Name = file.Name,
                    Size = file.Length
                });

                node.AddSize(file.Length);
            }

            foreach (var subDir in di.GetDirectories())
            {
                if (token.IsCancellationRequested) break;
                if (IsSymlink(subDir)) continue;

                var subNode = new DirectoryNode
                {
                    Name = subDir.Name,
                    FullPath = subDir.FullName
                };

                node.Children.Add(subNode);

                Interlocked.Increment(ref _pendingTasks);
                _queue.Enqueue(subNode);
            }
        }
        catch (Exception)
        {
        }
    }

    private static bool IsSymlink(FileSystemInfo info)
    {
        return info.LinkTarget != null || info.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    private long CalculateFinalSizes(DirectoryNode node)
    {
        long totalSize = node.Size; 

        foreach (var child in node.Children.OfType<DirectoryNode>())
        {
            totalSize += CalculateFinalSizes(child);
        }

        node.Size = totalSize;

        foreach (var child in node.Children)
        {
            child.Percentage = totalSize > 0
                ? (double)child.Size / totalSize * 100
                : 0;
        }

        return totalSize;
    }
}