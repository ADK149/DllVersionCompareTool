using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DllVersionCompareTool.Models;

namespace DllVersionCompareTool.Services;

/// <summary>
/// 扫描指定文件夹下的所有 .dll 文件，并读取大小 / 版本号。
/// </summary>
public static class DllScanner
{
    /// <summary>
    /// 扫描指定文件夹下的 .dll 文件。
    /// </summary>
    /// <param name="folderPath">文件夹路径</param>
    /// <param name="includeSubdirectories">是否包含所有子项（递归搜索）</param>
    public static List<DllInfo> Scan(string folderPath, bool includeSubdirectories = false)
    {
        var result = new List<DllInfo>();

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return result;

        var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        string[] files;
        try
        {
            files = Directory.GetFiles(folderPath, "*.dll", searchOption);
        }
        catch
        {
            return result;
        }

        foreach (var file in files)
        {
            var fi = new FileInfo(file);

            string version = string.Empty;
            try
            {
                var vi = FileVersionInfo.GetVersionInfo(file);
                if (!string.IsNullOrEmpty(vi.FileVersion))
                    version = vi.FileVersion;
                else if (!string.IsNullOrEmpty(vi.ProductVersion))
                    version = vi.ProductVersion;
            }
            catch
            {
                version = string.Empty;
            }

            result.Add(new DllInfo
            {
                FileName = fi.Name,
                FullPath = fi.FullName,
                Size = fi.Length,
                Version = version,
            });
        }

        return result;
    }
}
