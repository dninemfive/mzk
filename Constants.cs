using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.mzk
{
    public static class Constants
    {
        public const string BasePath = @"C:\Users\dninemfive\Music",
                            NumberSeperator = " - ",
                            Files = @"Files",
                            Unsorted = @"Unsorted",
                            Playlists = @"zzz_Playlists",
                            foobar2000 = @"_foobar2000";
        
        public static readonly List<string> ExtensionsToDelete = new() { ".jpg", ".db", ".ini" }, 
                                            PlaylistExtensions = new() { ".m3u8" };
        public static readonly List<string> IgnoreFolders = new()
        {
            Files,
            Unsorted,
            Playlists,
            foobar2000
        };
    }
}
