using System.Collections.ObjectModel;
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

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
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

        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(
            new SortDescription(nameof(CompareResultItem.FileName), ListSortDirection.Ascending));
    }
}
