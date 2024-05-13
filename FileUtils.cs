using d9.utl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.mzk;
internal static class FileUtils
{
    public static void DeleteIf(this string path, bool notDryRun)
    {
        MzkLog.Message(path, LogType.Delete);
        if (notDryRun)
            File.Delete(path);
    }
    public static bool HasPlaylistExtension(this string filePath)
        => Constants.PlaylistExtensions.Contains(Path.GetExtension(filePath));
    public static void MoveToUnsortedIf(this string oldPath, bool notDryRun)
    {
        string newPath = Path.Join(Constants.BasePath,
                                   Constants.UnsortedFolder,
                                   oldPath.RelativeTo(Constants.BasePath))
                             .NonConflictingFileName();
        MzkLog.Move(oldPath, newPath);
        if (notDryRun)
            oldPath.CreateFolderAndMoveTo(newPath);
    }
    public static string NonConflictingFileName(this string path)
    {
        int ct = 0;
        string fileName = Path.GetFileNameWithoutExtension(path),
               extension = Path.GetExtension(path);
        while (File.Exists(path))
            path = $"{fileName} ({++ct}){extension}";
        return path;
    }
    internal static string RelativeTo(this string path, string basePath)
        => Path.GetRelativePath(basePath, path);
    public static bool ShouldBeIgnored(this string path, bool print = false)
    {
        bool result = Constants.IgnoreFolders.Any(x => path.IsInFolder(Path.Join(Constants.BasePath, x)));
        if (result && print)
            MzkLog.Message(path, LogType.Ignore);
        return result;
    }
    public static bool ShouldDeleteExtension(this string path)
        => Constants.ExtensionsToDelete.Contains(Path.GetExtension(path));
    public static void CreateFolderAndMoveTo(this string oldPath, string newPath)
    {
        _ = Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
        File.Move(oldPath, newPath);
    }
}