using System.Collections.Generic;

namespace DllVersionCompareTool.Models;

/// <summary>
/// 单个 DLL 文件的基础信息（大小、版本等）
/// </summary>
public class DllInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// 表格中的一行结果
/// </summary>
public class CompareResultItem
{
    public int Number { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// 单次比较的输出（"比较结果" 列 + "备注" 列）
/// </summary>
public class CompareSummary
{
    public string Result { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public CompareSummary() { }

    public CompareSummary(string result, string notes)
    {
        Result = result;
        Notes = notes;
    }
}
