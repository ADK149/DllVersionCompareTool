using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using DllVersionCompareTool.Models;

namespace DllVersionCompareTool.ViewModels;

/// <summary>
/// "文件夹组" - 包含多个 <see cref="FolderItemViewModel"/>，
/// 同时保存本组比较后的 <see cref="CompareResultItem"/> 集合。
/// </summary>
public class FolderGroupViewModel : ObservableObject
{
    private string _name = "新组";
    private bool _includeSubdirectories = false;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// 是否包含所有子项（递归搜索子目录下的 dll 文件）
    /// </summary>
    public bool IncludeSubdirectories
    {
        get => _includeSubdirectories;
        set => SetProperty(ref _includeSubdirectories, value);
    }

    public ObservableCollection<FolderItemViewModel> Folders { get; } = new();

    public ObservableCollection<CompareResultItem> Results { get; } = new();

    /// <summary>
    /// 表格绑定的视图：默认按 "文件名" 升序排序。
    /// </summary>
    public ICollectionView ResultsView { get; }

    public FolderGroupViewModel()
    {
        // 默认添加两个文件夹选择组件
        Folders.Add(new FolderItemViewModel());
        Folders.Add(new FolderItemViewModel());
        RefreshFolderIndexes();

        Folders.CollectionChanged += OnFoldersCollectionChanged;

        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(
            new SortDescription(nameof(CompareResultItem.FileName), ListSortDirection.Ascending));
    }

    private void OnFoldersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshFolderIndexes();
    }

    private void RefreshFolderIndexes()
    {
        for (int i = 0; i < Folders.Count; i++)
        {
            Folders[i].Index = i + 1;
        }
    }
}
