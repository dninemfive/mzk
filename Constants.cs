using System.Collections.Generic;

namespace d9.mzk;

public static class Constants
{
    public const string BasePath = @"C:\Users\dninemfive\Music",
                        NumberSeperator = " - ",
                        Files = @"Files",
                        UnsortedFolder = @"Unsorted",
                        Playlists = @"zzz_Playlists",
                        foobar2000 = @"_foobar2000";
    
    public static readonly List<string> ExtensionsToDelete = new() { ".jpg", ".db", ".ini" }, 
                                        PlaylistExtensions = new() { ".m3u8" };
    public static readonly List<string> IgnoreFolders = new()
    {
        Files,
        UnsortedFolder,
        Playlists,
        foobar2000
    };
    public const string DeletePrefix    =  "! DELETE !",
                        IgnorePrefix    =  "@ IGNORE @",
                        MovePrefix      =  ">> MOVE >>",
                        WarnPrefix      = @"/!\WARN/!\";
}
