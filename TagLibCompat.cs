using d9.utl;
using System;
using System.IO;
using TagLibFile = TagLib.File;
using Tag = TagLib.Tag;

namespace d9.mzk;

internal static class TagUtils
{
    /// <summary>
    /// Takes a path to a song file and moves it to a location based on its metadata (see NewFilePath() for details)
    /// Done as an atomic operation to ensure playlists are consistent.
    /// </summary>
    /// <param name="oldPath"></param>
    public static void MoveSong(string oldPath)
    {
        TagLibFile? file = oldPath.GetTags();
        if (file is null)
            return;
        string newPath = file.Tag.NewPath(oldPath);
        // file system is case-insensitive on Windows
        if (newPath.ToLower() == oldPath.ToLower() || File.Exists(newPath))
            return;
        try
        {
            MzkLog.Move(oldPath, newPath);
            if (!Program.DryRun)
            {
                oldPath.MoveFileTo(newPath);
                Program.NewFileNames[oldPath] = newPath;
            }
        }
        catch (Exception e)
        {
            MzkLog.Error($"Copy from {oldPath} to {newPath} failed!", e);
            return;
        }
    }
    public static TagLibFile? GetTags(this string path, bool printCaughtError = false)
    {
        try
        {
            return TagLibFile.Create(path);
        } 
        catch(Exception e)
        {
            MzkLog.Warn($"File {path} does not have any music tags. Moving to unsorted...");
            if (printCaughtError)
                MzkLog.Error(e);
            path.MoveToUnsortedIf(!Program.DryRun);
            return null;
        }
    }
    // /Music/Files/[artist]/[album]/[disc number].[song number] - <song name>.<ext>
    static string NewDirectory(this Tag t)
    {
        string result = Path.Join(Constants.BasePath, Constants.Files, t.Artist().Trim().PathSafe());
        // https://stackoverflow.com/a/4123152
        if (!t.Album.NullOrEmpty()) result = Path.Join(result, t.Album.PathSafe().Trim('.')); 
        return result;
    }
    static string NewFileName(this Tag t, string oldPath)
    {
        string oldName = Path.GetFileName(oldPath),
               ext = Path.GetExtension(oldPath),
               newName = "";
        if (t.Disc != 0)
        {
            newName += $"{t.Disc}.{t.Track}{Constants.NumberSeperator}";
        }
        else if (t.Track != 0)
        {
            newName += $"{t.Track}{Constants.NumberSeperator}";
        }
        newName += t.Title;
        if (newName.Length < 1)
            return oldName;
        return newName.PathSafe() + ext;
    }
    public static string NewPath(this Tag t, string oldPath) 
        => Path.Join(t.NewDirectory(), t.NewFileName(oldPath));
    static string Artist(this Tag t) 
        => Utils.Sieve((x) => !string.IsNullOrEmpty(x), "_", t.JoinedAlbumArtists, t.JoinedPerformers, t.JoinedComposers);
}
