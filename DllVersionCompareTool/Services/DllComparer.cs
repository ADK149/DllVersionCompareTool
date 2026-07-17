using System.Collections.Generic;
using System.Linq;
using System.Text;
using DllVersionCompareTool.Models;

namespace DllVersionCompareTool.Services;

/// <summary>
/// DLL 文件比较器。
/// 在 "开始比较" 时，MainViewModel 会针对每个 DLL 文件名，
/// 从"文件夹组"中的若干个文件夹收集到一组 DllInfo（或 null），
/// 然后调用本类的 <see cref="Compare"/> 方法得到一行表格结果。
/// </summary>
public static class DllComparer
{
    /// <summary>
    /// 比较多个文件夹中同一个 DLL 文件的信息。
    /// </summary>
    /// <param name="fileInfos">
    /// 按"文件夹组"中文件夹顺序排列的 DllInfo 列表。
    /// 若某个文件夹没有该 DLL，则对应位置为 null。
    /// 列表长度 == 该组中文件夹的数量。
    /// 每个非 null 项可用字段：<c>FileName</c>、<c>Size</c>、<c>Version</c>、<c>FullPath</c>。
    /// </param>
    /// <returns>
    /// <see cref="CompareSummary"/>：<c>Result</c> 显示在表格"比较结果"列；<c>Notes</c> 显示在"备注"列。
    /// </returns>
    public static CompareSummary Compare(List<DllInfo?> fileInfos)
    {
        // ====================================================================
        // TODO: 用户将在此方法中实现自己的比较逻辑
        //
        //  入参: fileInfos - 按文件夹顺序排列的 DllInfo 列表 (可能含 null)
        //  字段: FileName / Size / Version / FullPath
        //  返回: CompareSummary(Result, Notes)
        //        Result -> 显示在"比较结果"列 (建议简短, 如 "相同" / "不同")
        //        Notes  -> 显示在"备注"列     (可放详细信息)
        //
        //  以下是默认实现 (按 Size + Version 判断):
        // ====================================================================

        var present = fileInfos.Where(i => i != null).Cast<DllInfo>().ToList();

        if (present.Count == 0)
            return new CompareSummary("缺失", "所有文件夹均未找到该文件");

        if (present.Count < fileInfos.Count)
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < fileInfos.Count; i++)
            {
                var info = fileInfos[i];
                stringBuilder.Append(info != null
                    ? $"文件夹{i + 1}: {info.Size} 字节, 版本 {info.Version}"
                    : $"文件夹{i + 1}: 缺失");
                if (i < fileInfos.Count - 1) stringBuilder.AppendLine();
            }
            return new CompareSummary("缺失", $"仅 {present.Count}/{fileInfos.Count} 个文件夹包含此文件\n{stringBuilder.ToString()}");
        }
            

        var first = present[0];
        bool allSame = present.All(i => i.Size == first.Size && i.Version == first.Version);

        if (allSame)
        {
            return new CompareSummary(
                "相同",
                $"大小: {first.Size} 字节; 版本: {first.Version}");
        }

        var sb = new StringBuilder();
        for (int i = 0; i < fileInfos.Count; i++)
        {
            var info = fileInfos[i];
            sb.Append(info != null
                ? $"文件夹{i + 1}: {info.Size} 字节, 版本 {info.Version}"
                : $"文件夹{i + 1}: 缺失");
            if (i < fileInfos.Count - 1) sb.AppendLine();
        }
        return new CompareSummary("不同", sb.ToString());
    }
}
