namespace DllVersionCompareTool.ViewModels;

/// <summary>
/// "选择文件夹的组件" - 仅保存一个文件夹路径。
/// </summary>
public class FolderItemViewModel : ObservableObject
{
    private string _folderPath = string.Empty;

    public string FolderPath
    {
        get => _folderPath;
        set => SetProperty(ref _folderPath, value);
    }
}
