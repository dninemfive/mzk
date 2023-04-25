using System;
using System.IO;
using TagLib;
using System.Linq;
using System.Collections.Generic;

namespace d9.mzk
{    
    static class Program
    {
        public static bool DryRun { get; private set; } = false;
        public static Dictionary<string, string> newFileNames = new();        
        static void Main(string[] args)
        {
            if (args.Contains("--dryrun")) DryRun = true;
            if (args.Contains("--resort")) Constants.IgnoreFolders.RemoveAt(0);
            Utils.OpenLog();
            try
            {
                MoveSongsIn(Constants.BasePath);
                UpdatePlaylists();
                Utils.DeleteEmptyFolders();
            } 
            finally
            {
                Utils.CloseLog();
            }           
        }
        public static void MoveSongsIn(string folder)
        {            
            foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (Utils.ShouldIgnore(file))
                {
                    // Utils.WriteLine($"@ IGNORE: {file}");
                    continue;
                }
                if (Constants.ExtensionsToDelete.Contains(Path.GetExtension(file)))
                {
                    Utils.WriteLine($"! DELETE ! {file}");
                    if(!DryRun) System.IO.File.Delete(file);
                    continue;
                }
                MoveSong(file);
            }
        }
        /// <summary>
        /// Takes a path to a song file and moves it to a location based on its metadata (see NewFilePath() for details)
        /// Done as an atomic operation to ensure playlists are consistent.
        /// </summary>
        /// <param name="oldPath"></param>
        static void MoveSong(string oldPath)
        {
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(oldPath);
            }
            catch (Exception e)
            {
                if(!DryRun) oldPath.MoveToUnsorted();
                Utils.WriteLine($"!! ERR  !! Caught exception {e.Message} while attempting to move {oldPath}. Moving to unsorted...");
                return;
            }
            string newPath = NewPath(file.Tag, oldPath);
            // file system is case-insensitive on Windows
            if (newPath.ToLower() == oldPath.ToLower()) return;
            if(System.IO.File.Exists(newPath)) return;
            try
            {
                Utils.WriteLine($">  MOVE  > {oldPath}\n         ↪ {newPath}");
                if (!DryRun) oldPath.MoveTo(newPath);
            }
            catch(Exception e)
            {
                Utils.WriteLine($"!! ERR  !! Copy from {oldPath} to {newPath} failed ({e.Message}).");
                return;
            }
            newFileNames[oldPath] = newPath;
        }
        // /Music/Files/[artist]/[album]/[disc number].[song number] - <song name>.<ext>
        static string NewDirectory(Tag t)
        {
            // [artist]
            string newPath = Path.Join(Constants.BasePath, Constants.Files, t.Artist().Trim()) + @"\";
            // [album]
            string album;
            if ((album = t.Album()) is not null) newPath += album.Safe() + @"\";
            return newPath.Safe(directory: true);
        }
        static string NewFileName(Tag t, string oldPath)
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
            return newName.Safe() + ext;
        }
        public static string NewPath(Tag t, string oldPath) => Path.Join(NewDirectory(t), NewFileName(t, oldPath));
        static string Artist(this Tag t) => Sieve((x) => !string.IsNullOrEmpty(x), "_", t.JoinedAlbumArtists, t.JoinedPerformers, t.JoinedComposers);
        static string Album(this Tag t)
        {
            if (!t.Album.NullOrEmpty()) return t.Album;
            return null;
        }
        public static bool NullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        public static T Sieve<T>(Func<T, bool> lambda, T @default, params T[] ts)
            => ts.FirstOrDefault(x => lambda(x), @default);
        public static string Safe(this string s, bool directory = false)
        {
            string ret = s;
            foreach (char c in Constants.ForbiddenCharacters)
            {
                if (directory && (c is '\\' or '/')) continue;
                ret = ret.Replace(c, '_');
            }
            while(ret.Last() == '.')
            {
                ret = ret[0..^1];
            }
            if (directory) ret = @"C:\" + ret[3..];
            while (ret.Contains(@"\\")) ret = ret.Replace(@"\\", @"\");
            return ret.Trim();
        }
        static void UpdatePlaylists()
        {
            foreach (string s in Directory.EnumerateFiles(Path.Join(Constants.BasePath, Constants.Playlists))) UpdatePlaylist(s);
        }
        static void UpdatePlaylist(string filename)
        {
            if (!Constants.PlaylistExtensions.Contains(Path.GetExtension(filename))) return;
            List<string> text = new();
            foreach(string s in System.IO.File.ReadLines(filename))
            {
                if(newFileNames.TryGetValue(s, out string value))
                {
                    text.Add(value);
                } else
                {
                    text.Add(s);
                }
            }
            string toWrite = "";
            foreach(string s in text)
            {
                toWrite += $"{s}\n";
            }
            System.IO.File.WriteAllText(filename, toWrite);
        }        
    }
}
