using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    public RelayCommand StartCompareCommand { get; }

    private int _groupCounter = 0;

    public MainViewModel()
    {
        AddGroupCommand = new RelayCommand(_ => AddGroup());
        RemoveGroupCommand = new RelayCommand(p => RemoveGroup(p as FolderGroupViewModel));
        AddFolderCommand = new RelayCommand(p => AddFolder(p as FolderGroupViewModel));
        RemoveFolderCommand = new RelayCommand(p => RemoveFolder(p as FolderItemViewModel));
        BrowseFolderCommand = new RelayCommand(p => BrowseFolder(p as FolderItemViewModel));
        StartCompareCommand = new RelayCommand(_ => StartCompare());

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

    private void StartCompare()
    {
        foreach (var group in Groups)
        {
            group.Results.Clear();

            // 1. 扫描每个文件夹，建立 "文件名 -> DllInfo" 映射，并收集所有出现过的文件名
            var perFolderDicts = new List<Dictionary<string, DllInfo>>(group.Folders.Count);
            var allNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var folderVm in group.Folders)
            {
                var infos = DllScanner.Scan(folderVm.FolderPath, group.IncludeSubdirectories);
                var dict = new Dictionary<string, DllInfo>(infos.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var info in infos)
                {
                    dict[info.FileName] = info;
                    allNames.Add(info.FileName);
                }
                perFolderDicts.Add(dict);
            }

            if(allNames.Count == 0)
            {
                group.Results.Add(new CompareResultItem
                {
                    Number = 1,
                    FileName = "",
                    Result = "未比较",
                    Notes = "所有文件夹内未找到.dll文件"
                });
            }

            // 2. 按文件名排序后逐个调用 DllComparer 比较
            int number = 1;
            foreach (var name in allNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
            {
                var perFolder = group.Folders
                    .Select((_, i) => perFolderDicts[i].TryGetValue(name, out var v) ? (DllInfo?)v : null)
                    .ToList();

                // ====== 调用"用户可自定义"的比较方法 ======
                var summary = DllComparer.Compare(perFolder);
                // =========================================

                group.Results.Add(new CompareResultItem
                {
                    Number = number++,
                    FileName = name,
                    Result = summary.Result,
                    Notes = summary.Notes,
                });
            }
        }
    }
}
