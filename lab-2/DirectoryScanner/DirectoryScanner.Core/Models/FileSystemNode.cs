using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DirectoryScanner.Core.Models;

public abstract class FileSystemNode : INotifyPropertyChanged
{
    public string Name { get; set; } = string.Empty;
    private long _size;

    public long Size
    {
        get => _size;
        set
        {
            _size = value;
            OnPropertyChanged();
        }
    }

    public void AddSize(long value)
    {
        Interlocked.Add(ref _size, value);
        OnPropertyChanged(nameof(Size));
    }

    private double _percentage;
    public double Percentage
    {
        get => _percentage;
        set { _percentage = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class FileNode : FileSystemNode { }

public class DirectoryNode : FileSystemNode
{
    public string FullPath { get; set; } = string.Empty;
    public ConcurrentBag<FileSystemNode> Children { get; } = new();
}