using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace d9.mzk
{
    internal static class TagUtils
    {
        /// <summary>
        /// Takes a path to a song file and moves it to a location based on its metadata (see NewFilePath() for details)
        /// Done as an atomic operation to ensure playlists are consistent.
        /// </summary>
        /// <param name="oldPath"></param>
        public static void MoveSong(string oldPath)
        {
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(oldPath);
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
            if (File.Exists(newPath)) return;
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
            if(t.Title == "My World Is Empty Without You")
            {
                byte[] bytes = File.ReadAllBytes(oldPath);
                string filename = "test.txt";
                File.WriteAllText(filename, "");
                StreamWriter writer = File.AppendText(filename);
                int progressInterval = bytes.Length / 100;
                Program.Log.WriteLine($"Total bytes: {bytes.Length}; interval: {progressInterval}");
                foreach ((byte b, int i) in bytes.WithProgress(Program.Log))
                {
                    writer.Write(b switch
                    {
                        >= (byte)'a' and <= (byte)'z' => (char)b,
                        >= (byte)'A' and <= (byte)'Z' => (char)b,
                        >= 1 and <= 9 => (char)('0' + b),
                        0 => '_',
                        _ => b.ToHex()
                    });
                    if (i % 128 == 0) writer.Write("\n");
                }
            }
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
}
