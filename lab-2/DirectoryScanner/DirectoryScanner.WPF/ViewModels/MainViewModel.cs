using DirectoryScanner.Core.Models;
using DirectoryScanner.Core.Services;
using Microsoft.Win32; 
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DirectoryScanner.WPF.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private CancellationTokenSource? _cts;
    private bool _isScanning;
    private string _status = "Готов к работе";

    public ObservableCollection<DirectoryNode> RootNodes { get; } = new();

    public bool IsScanning
    {
        get => _isScanning;
        set { _isScanning = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public ICommand StartCommand { get; }
    public ICommand CancelCommand { get; }

    public MainViewModel()
    {
        StartCommand = new RelayCommand(async _ => await StartScan(), _ => !IsScanning);
        CancelCommand = new RelayCommand(_ => _cts?.Cancel(), _ => IsScanning);
    }

    private async Task StartScan()
    {

        var dialog = new OpenFolderDialog
        {
            Title = "Выберите папку для анализа",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() != true) return;

        string selectedPath = dialog.FolderName;
        _cts = new CancellationTokenSource();

        var engine = new ScannerEngine(Environment.ProcessorCount * 2);

        IsScanning = true;
        Status = $"Сканирование: {selectedPath}";
        RootNodes.Clear();

        try
        {
            var result = await engine.ScanAsync(selectedPath, _cts.Token);

            Application.Current.Dispatcher.Invoke(() => {
                RootNodes.Add(result);
            });

            Status = _cts.IsCancellationRequested ? "Сканирование прервано (показаны частичные данные)" : "Готово";
        }
        catch (UnauthorizedAccessException)
        {
            Status = "Ошибка: нет доступа к папке";
            MessageBox.Show("У приложения недостаточно прав для сканирования этой директории.", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            Status = "Критическая ошибка";
            MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsScanning = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}