using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace d9.mzk.compat.TagLib
{
    internal static class TagLibUtils
    {
        /// <summary>
        /// Takes a path to a song file and moves it to a location based on its metadata (see NewFilePath() for details)
        /// Done as an atomic operation to ensure playlists are consistent.
        /// </summary>
        /// <param name="oldPath"></param>
        public static void MoveSong(string oldPath)
        {
            File file;
            try
            {
                file = File.Create(oldPath);
            }
            catch (Exception e)
            {
                if (!Program.DryRun) oldPath.MoveToUnsorted();
                Program.Log.WriteLine($"!! ERR  !! Caught exception {e.Message} while attempting to move {oldPath}. Moving to unsorted...");
                return;
            }
            string newPath = NewPath(file.Tag, oldPath);
            // file system is case-insensitive on Windows
            if (newPath.ToLower() == oldPath.ToLower()) return;
            if (System.IO.File.Exists(newPath)) return;
            try
            {
                Program.Log.WriteLine($">  MOVE  > {oldPath}\n         ↪ {newPath}");
                if (!Program.DryRun) oldPath.MoveFileTo(newPath);
            }
            catch (Exception e)
            {
                Program.Log.WriteLine($"!! ERR  !! Copy from {oldPath} to {newPath} failed ({e.Message}).");
                return;
            }
            Program.NewFileNames[oldPath] = newPath;
        }
        // /Music/Files/[artist]/[album]/[disc number].[song number] - <song name>.<ext>
        static string NewDirectory(Tag t)
        {
            // [artist]
            string newPath = System.IO.Path.Join(Constants.BasePath, Constants.Files, t.Artist().Trim()) + @"\";
            // [album]
            string album;
            if ((album = t.Album()) is not null) newPath += album.Safe() + @"\";
            return newPath.Safe(directory: true);
        }
        static string NewFileName(Tag t, string oldPath)
        {
            string oldName = System.IO.Path.GetFileName(oldPath),
                   ext = System.IO.Path.GetExtension(oldPath),
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
            return newName.Safe() + ext;
        }
        public static string NewPath(Tag t, string oldPath) => System.IO.Path.Join(NewDirectory(t), NewFileName(t, oldPath));
        static string Artist(this Tag t) => Program.Sieve((x) => !string.IsNullOrEmpty(x), "_", t.JoinedAlbumArtists, t.JoinedPerformers, t.JoinedComposers);
        static string Album(this Tag t)
        {
            if (!t.Album.NullOrEmpty()) return t.Album;
            return null;
        }
    }
}
