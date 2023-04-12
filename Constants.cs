using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzk
{
    public static class Constants
    {
        public const string BasePath = @"C:\Users\dninemfive\Music\",
                            NumberSeperator = " - ",
                            Files = @"Files\",
                            Unsorted = @"Unsorted\",
                            Playlists = @"zzz_Playlists\",
                            foobar2000 = @"_foobar2000\";
        public static readonly string ForbiddenCharacters = @"<>:/\|?*" + '"';
        public static readonly List<string> ExtensionsToDelete = new() { ".jpg", ".db", ".ini" }, 
                                            PlaylistExtensions = new() { ".m3u8" };
        public static readonly List<string> IgnoreFolders = new()
        {
            BasePath + Files,
            BasePath + Unsorted,
            BasePath + Playlists,
            BasePath + foobar2000
        };
    }
}
