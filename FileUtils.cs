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
        Program.Log?.WriteLine($"{Constants.DeletePrefix} {path}");
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
        Program.Log?.WriteLine($"{Constants.MovePrefix} {oldPath} -> {newPath}");
        if (notDryRun)
            File.Move(oldPath, newPath);
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
    public static bool ShouldBeIgnored(this string path)
    {
        bool result = Constants.IgnoreFolders.Any(x => path.IsInFolder(Path.Join(Constants.BasePath, x)));
        if (result)
            Program.Log?.WriteLine($"{Constants.IgnorePrefix} {path}");
        return result;
    }
    public static bool ShouldDeleteExtension(this string path)
        => Constants.ExtensionsToDelete.Contains(Path.GetExtension(path));
}
