using d9.utl;
using System;
using System.IO;
using TagLibFile = TagLib.File;

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
        string newPath = NewPath(file.Tag, oldPath);
        // file system is case-insensitive on Windows
        if (newPath.ToLower() == oldPath.ToLower() || File.Exists(newPath))
            return;
        try
        {
            Program.Log.WriteLine($">  MOVE  > {oldPath}\n         ↪ {newPath}");
            if (!Program.DryRun)
            {
                oldPath.MoveFileTo(newPath);
                Program.NewFileNames[oldPath] = newPath;
            }
        }
        catch (Exception e)
        {
            Program.Log.WriteLine($"!! ERR  !! Copy from {oldPath} to {newPath} failed ({e.Message}).");
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
            Program.Log.WriteLine($"/!\\WRN/!\\ File {path} does not have any music tags. Moving to unsorted...");
            if (printCaughtError)
                Program.Log.WriteLine($"!! ERR !! Exception: {e.Message}");
            path.MoveToUnsortedIf(!Program.DryRun);
            return null;
        }
    }
    // /Music/Files/[artist]/[album]/[disc number].[song number] - <song name>.<ext>
    static string NewDirectory(TagLib.Tag t)
    {
        string result = Path.Join(Constants.BasePath, Constants.Files, t.Artist().Trim().PathSafe());
        if (!t.Album.NullOrEmpty()) result = Path.Join(result, t.Album.PathSafe()); 
        return result;
    }
    static string NewFileName(TagLib.Tag t, string oldPath)
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
        if (newName.Length < 1) return oldName;
        return newName.PathSafe() + ext;
    }
    public static string NewPath(TagLib.Tag t, string oldPath) => Path.Join(NewDirectory(t), NewFileName(t, oldPath));
    static string Artist(this TagLib.Tag t) => Utils.Sieve((x) => !string.IsNullOrEmpty(x), "_", t.JoinedAlbumArtists, t.JoinedPerformers, t.JoinedComposers);
}
