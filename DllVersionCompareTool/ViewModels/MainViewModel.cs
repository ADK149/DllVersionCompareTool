using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DllVersionCompareTool.Models;
using DllVersionCompareTool.Services;
using Microsoft.Win32;

namespace DllVersionCompareTool.ViewModels;

public class MainViewModel : ObservableObject
{
    public ObservableCollection<FolderGroupViewModel> Groups { get; } = new();

    public RelayCommand AddGroupCommand { get; }
    public RelayCommand RemoveGroupCommand { get; }
    public RelayCommand AddFolderCommand { get; }
    public RelayCommand RemoveFolderCommand { get; }
    public RelayCommand BrowseFolderCommand { get; }
    public AsyncRelayCommand StartCompareCommand { get; }

    private int _groupCounter = 0;

    private string _statusText = "状态：未开始";

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public MainViewModel()
    {
        AddGroupCommand = new RelayCommand(_ => AddGroup());
        RemoveGroupCommand = new RelayCommand(p => RemoveGroup(p as FolderGroupViewModel));
        AddFolderCommand = new RelayCommand(p => AddFolder(p as FolderGroupViewModel));
        RemoveFolderCommand = new RelayCommand(p => RemoveFolder(p as FolderItemViewModel));
        BrowseFolderCommand = new RelayCommand(p => BrowseFolder(p as FolderItemViewModel));
        StartCompareCommand = new AsyncRelayCommand(_ => StartCompareAsync());

        // 默认给一个组，方便用户直接添加文件夹开始比较
        AddGroup();
    }

    private void AddGroup()
    {
        _groupCounter++;
        Groups.Add(new FolderGroupViewModel { Name = $"组{_groupCounter}" });
    }

    private void RemoveGroup(FolderGroupViewModel? group)
    {
        if (group != null) Groups.Remove(group);
    }

    private void AddFolder(FolderGroupViewModel? group)
    {
        if (group != null) group.Folders.Add(new FolderItemViewModel());
    }

    private void RemoveFolder(FolderItemViewModel? folder)
    {
        if (folder == null) return;
        foreach (var g in Groups)
        {
            if (g.Folders.Remove(folder)) return;
        }
    }

    private void BrowseFolder(FolderItemViewModel? folder)
    {
        if (folder == null) return;

        var dialog = new OpenFolderDialog
        {
            Title = "选择文件夹",
            InitialDirectory = !string.IsNullOrEmpty(folder.FolderPath) && Directory.Exists(folder.FolderPath)
                ? folder.FolderPath
                : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        };

        if (dialog.ShowDialog() == true)
        {
            folder.FolderPath = dialog.FolderName;
        }
    }

    private async Task StartCompareAsync()
    {
        StatusText = "状态：扫描文件中...";

        foreach (var group in Groups)
        {
            group.Results.Clear();

            var folderPaths = group.Folders.Select(f => f.FolderPath).ToList();
            var includeSubdirs = group.IncludeSubdirectories;

            // 将扫描逻辑放到后台线程
            var scanResult = await Task.Run(() =>
            {
                var perFolderDicts = new List<Dictionary<string, DllInfo>>(folderPaths.Count);
                var allNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var folderPath in folderPaths)
                {
                    var infos = DllScanner.Scan(folderPath, includeSubdirs);
                    var dict = new Dictionary<string, DllInfo>(infos.Count, StringComparer.OrdinalIgnoreCase);
                    foreach (var info in infos)
                    {
                        dict[info.FileName] = info;
                        allNames.Add(info.FileName);
                    }
                    perFolderDicts.Add(dict);
                }

                return new { PerFolderDicts = perFolderDicts, AllNames = allNames };
            });

            StatusText = "状态：比对文件中...";

            var perFolderDicts = scanResult.PerFolderDicts;
            var allNames = scanResult.AllNames;

            if (allNames.Count == 0)
            {
                group.Results.Add(new CompareResultItem
                {
                    Number = 1,
                    FileName = "",
                    Result = "未比较",
                    Notes = "所有文件夹内未找到.dll文件"
                });
                continue;
            }

            // 比较逻辑也放到后台线程
            var compareResults = await Task.Run(() =>
            {
                var results = new List<CompareResultItem>();
                int number = 1;
                foreach (var name in allNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
                {
                    var perFolder = folderPaths
                        .Select((_, i) => perFolderDicts[i].TryGetValue(name, out var v) ? (DllInfo?)v : null)
                        .ToList();

                    var summary = DllComparer.Compare(perFolder);

                    results.Add(new CompareResultItem
                    {
                        Number = number++,
                        FileName = name,
                        Result = summary.Result,
                        Notes = summary.Notes,
                    });
                }
                return results;
            });

            foreach (var item in compareResults)
            {
                group.Results.Add(item);
            }
        }

        StatusText = "状态：比较结束";
    }
}
