using Godot;
using System;
using System.IO;

/// <summary>
/// 此代码文件主要处理对外部的文本类文件的 IO 操作（Godot 4.x 版本）。
/// </summary>
public partial class IOControl_MinLib : Node
{
    /// <summary>
    /// 使用 Godot 的 File 类检测指定路径下的文件是否存在。
    /// </summary>
    /// <param name="path">目标路径，例如 "user://test.txt"</param>
    /// <param name="debug">是否在调试终端输出信息，默认 false</param>
    /// <returns>如果文件存在返回 true，否则返回 false</returns>
    public static bool Exists(string path, bool debug = false)
    {
        // 将 Godot 路径转换为系统绝对路径
        string absolutePath = ProjectSettings.GlobalizePath(path);
        bool result = File.Exists(absolutePath) || Directory.Exists(absolutePath);
        if (debug)
            GD.Print($"[Exists] 路径：{absolutePath} 存在：{result}");
        return result;
    }
    
    /// <summary>
    /// 删除给定路径的文件或文件夹。
    /// 支持 Godot 路径格式（如 "user://my_folder" 或 "user://test.txt"），
    /// 通过 ProjectSettings.GlobalizePath 转换为绝对路径后进行删除操作。
    /// 注意：res:// 下的文件一般只读，不可删除。
    /// </summary>
    /// <param name="path">目标路径</param>
    /// <param name="debug">是否在调试终端输出信息，默认 false</param>
    /// <returns>删除成功返回 true，否则返回 false</returns>
    public static bool DeletePath(string path, bool debug = false)
    {
        string absolutePath = ProjectSettings.GlobalizePath(path);

        try
        {
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                if (debug)
                    GD.Print($"[DeletePath] 文件已删除：{absolutePath}");
                return true;
            }
            else if (Directory.Exists(absolutePath))
            {
                Directory.Delete(absolutePath, true);
                if (debug)
                    GD.Print($"[DeletePath] 文件夹已删除：{absolutePath}");
                return true;
            }
            else
            {
                if (debug)
                    GD.Print($"[DeletePath] 删除失败：路径不存在 {absolutePath}");
                return false;
            }
        }
        catch (Exception ex)
        {
            if (debug)
                GD.PrintErr($"[DeletePath] 删除失败：{absolutePath}，异常：{ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 复制给定源文件夹到目标文件夹（包括所有子目录和文件）。
    /// 支持 Godot 路径格式（如 "user://source_folder"），
    /// 通过 ProjectSettings.GlobalizePath 转换为绝对路径。
    /// 如果目标文件夹不存在，则会自动创建。
    /// </summary>
    /// <param name="sourcePath">源文件夹路径，例如 "user://my_folder"</param>
    /// <param name="destinationPath">目标文件夹路径，例如 "user://backup_folder"</param>
    /// <param name="debug">是否在调试终端输出信息，默认 false</param>
    /// <returns>复制成功返回 true，否则返回 false</returns>
    public static bool CopyDirectory(string sourcePath, string destinationPath, bool debug = false)
    {
        string srcAbsolute = ProjectSettings.GlobalizePath(sourcePath);
        string destAbsolute = ProjectSettings.GlobalizePath(destinationPath);

        try
        {
            if (!Directory.Exists(srcAbsolute))
            {
                if (debug)
                    GD.PrintErr($"[CopyDirectory] 源文件夹不存在：{srcAbsolute}");
                return false;
            }

            if (!Directory.Exists(destAbsolute))
            {
                Directory.CreateDirectory(destAbsolute);
                if (debug)
                    GD.Print($"[CopyDirectory] 目标文件夹创建成功：{destAbsolute}");
            }

            // 复制当前目录下所有文件
            foreach (string file in Directory.GetFiles(srcAbsolute))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destAbsolute, fileName);
                File.Copy(file, destFile, true);
                if (debug)
                    GD.Print($"[CopyDirectory] 文件复制：{fileName}");
            }

            // 递归复制所有子目录
            foreach (string directory in Directory.GetDirectories(srcAbsolute))
            {
                string dirName = Path.GetFileName(directory);
                string destSubDir = Path.Combine(destAbsolute, dirName);
                CopyDirectory(directory, destSubDir, debug);
            }

            if (debug)
                GD.Print($"[CopyDirectory] 成功复制文件夹：从 {srcAbsolute} 到 {destAbsolute}");
            return true;
        }
        catch (Exception ex)
        {
            if (debug)
                GD.PrintErr($"[CopyDirectory] 复制文件夹失败：{ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 复制单个文件到指定路径。
    /// 支持 Godot 路径格式（如 "user://data.txt"），
    /// 通过 ProjectSettings.GlobalizePath 转换为系统绝对路径后进行复制操作。
    /// 如果目标目录不存在，会自动创建。
    /// </summary>
    /// <param name="sourceFilePath">源文件路径，例如 "user://data.txt"</param>
    /// <param name="destinationFilePath">目标文件路径，例如 "user://backup/data.txt"</param>
    /// <param name="debug">是否在调试终端输出信息，默认 false</param>
    /// <returns>复制成功返回 true，否则返回 false</returns>
    public static bool CopyFile(string sourceFilePath, string destinationFilePath, bool debug = false)
    {
        string srcAbsolute = ProjectSettings.GlobalizePath(sourceFilePath);
        string destAbsolute = ProjectSettings.GlobalizePath(destinationFilePath);

        try
        {
            if (!File.Exists(srcAbsolute))
            {
                if (debug)
                    GD.PrintErr($"[CopyFile] 源文件不存在：{srcAbsolute}");
                return false;
            }

            string destDirectory = Path.GetDirectoryName(destAbsolute);
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
                if (debug)
                    GD.Print($"[CopyFile] 目标目录创建成功：{destDirectory}");
            }

            File.Copy(srcAbsolute, destAbsolute, true);
            if (debug)
                GD.Print($"[CopyFile] 文件复制成功：从 {srcAbsolute} 到 {destAbsolute}");
            return true;
        }
        catch (Exception ex)
        {
            if (debug)
                GD.PrintErr($"[CopyFile] 复制文件失败：{ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 创建指定路径的文件夹。
    /// 支持 Godot 路径格式（如 "user://my_folder"），通过 ProjectSettings.GlobalizePath 转换为系统绝对路径后进行创建操作。
    /// 如果目录已存在，则不进行任何操作。
    /// </summary>
    /// <param name="path">目标文件夹路径，例如 "user://my_folder"</param>
    /// <param name="debug">是否在调试终端输出信息，默认 false</param>
    /// <returns>创建成功或目录已存在返回 true，否则返回 false</returns>
    public static bool CreateDirectory(string path, bool debug = false)
    {
        string absolutePath = ProjectSettings.GlobalizePath(path);

        try
        {
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
                if (debug)
                    GD.Print($"[CreateDirectory] 目录创建成功：{absolutePath}");
            }
            else
            {
                if (debug)
                    GD.Print($"[CreateDirectory] 目录已存在：{absolutePath}");
            }
            return true;
        }
        catch (Exception ex)
        {
            if (debug)
                GD.PrintErr($"[CreateDirectory] 创建目录失败：{ex.Message}");
            return false;
        }
    }
}
