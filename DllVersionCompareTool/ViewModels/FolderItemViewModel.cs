namespace DllVersionCompareTool.ViewModels;

/// <summary>
/// "选择文件夹的组件" - 仅保存一个文件夹路径。
/// </summary>
public class FolderItemViewModel : ObservableObject
{
    private int _index;
    private string _folderPath = string.Empty;

    /// <summary>
    /// 在所属组中的序号（从1开始）
    /// </summary>
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public string FolderPath
    {
        get => _folderPath;
        set => SetProperty(ref _folderPath, value);
    }
}
