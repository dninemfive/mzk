using System;
using System.IO;
using TagLib;
using System.Linq;
using System.Collections.Generic;

namespace musicsort
{    
    static class Program
    {
        public const string BASE_DIRECTORY_PATH = @"C:\Users\dninemfive\Music\", NUMBER_SEPARATOR = " - ", FILES = @"Files\", UNSORTED = @"Unsorted\", PLAYLISTS = @"zzz_Playlists\";
        public static readonly char[] FORBIDDEN_CHARS = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        public static readonly List<string> DELETE_FILE_EXTENSIONS = new List<string>() { ".jpg", ".db", ".ini" }, PLAYLIST_FILE_EXTENSIONS = new List<string>() { ".m3u8" };
        public static List<string> IgnoreFolders = new()
        {
            BASE_DIRECTORY_PATH + FILES,
            BASE_DIRECTORY_PATH + UNSORTED,
            BASE_DIRECTORY_PATH + PLAYLISTS,
            BASE_DIRECTORY_PATH + @"_foobar2000\"
        };
        public static Dictionary<string, string> newFileNames = new();
        static void Main()
        {
            MoveSongsIn(BASE_DIRECTORY_PATH);    
            UpdatePlaylists();
            DeleteEmptyFolders();
        }
        public static void MoveSongsIn(string folder)
        {
            if (IgnoreFolders.Contains(folder + @"\")) return;
            foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (DELETE_FILE_EXTENSIONS.Contains(Path.GetExtension(file)))
                {
                    System.IO.File.Delete(file);
                    continue;
                }
                MoveSong(file);
            }
        }
        /// <summary>
        /// Takes a path to a song file and moves it to a location based on its metadata (see NewFilePath() for details)
        /// Done as an atomic operation to ensure playlists are consistent.
        /// If the song is already present at the destination (duplicated somewhere?) it 
        /// </summary>
        /// <param name="oldPath"></param>
        static void MoveSong(string oldPath)
        {
            // load file at path
            Console.WriteLine($"Moving {oldPath}...");
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(oldPath);
            }
            catch (Exception e)
            {
                string targetPath = BASE_DIRECTORY_PATH + UNSORTED + oldPath.Replace(BASE_DIRECTORY_PATH, "");
                // if not supported by taglib, move to temp folder for me to manually move
                if(!System.IO.File.Exists(targetPath))
                    System.IO.File.Copy(oldPath, targetPath);
                System.IO.File.Delete(oldPath);
                Console.WriteLine(e);
                return;
            }
            string newPath = NewDirPath(file.Tag);            
            if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);
            newPath += NewFileName(file.Tag, Path.GetExtension(oldPath), Path.GetFileName(oldPath));
            if (newPath == oldPath) return;
            Console.WriteLine(newPath);            
            // attempt to move to that path
            if(System.IO.File.Exists(newPath))
            {
                Console.WriteLine("Copy from " + oldPath + " to " + newPath + " failed (file already exists)");
                return;
            }
            try
            {
                System.IO.File.Copy(oldPath, newPath);
            }
            catch(Exception e)
            {
                // otherwise, error or smth idk
                Console.WriteLine("Copy from " + oldPath + " to " + newPath + " failed (" + e + ")");
                return;
            }
            // if successful, go through all playlists and update the path
            newFileNames[oldPath] = newPath;
            System.IO.File.Delete(oldPath);
        }
        // /Music/Files/[artist]/[album]/[disc number].[song number] - <song name>.<ext>
        static string NewDirPath(Tag t)
        {
            // [artist]
            string newPath = BASE_DIRECTORY_PATH + FILES + t.GetArtist().Trim() + @"\";
            // [album]
            string s;
            if ((s = t.GetAlbum()) != null) newPath += s.Safe() + @"\";
            return newPath.Safe(directory: true);
        }
        static string NewFileName(Tag t, string ext, string oldName)
        {
            string newName = "";
            // [disc number].[song number] - `
            if (t.Disc != 0)
            {
                newName += t.Disc + "." + t.Track + NUMBER_SEPARATOR;
            }
            else if (t.Track != 0)
            {
                newName += t.Track + NUMBER_SEPARATOR;
            }
            // <song name>.<ext>
            newName += t.Title;
            if (newName.Length < 1) return oldName;
            return newName.Safe() + ext;
        }
        static string GetArtist(this Tag t)
        {
            if (!t.JoinedAlbumArtists.NullOrEmpty()) return t.JoinedAlbumArtists;
            if (!t.JoinedPerformers.NullOrEmpty()) return t.JoinedPerformers;
            if (!t.JoinedComposers.NullOrEmpty()) return t.JoinedComposers;
            return "_";
        }
        static string GetAlbum(this Tag t)
        {
            if (!t.Album.NullOrEmpty()) return t.Album;
            return null;
        }
        public static bool NullOrEmpty(this string s)
        {
            return !(s?.Length > 0);
        }
        public static string Safe(this string s, bool directory = false)
        {
            string ret = s;
            foreach (char c in FORBIDDEN_CHARS)
            {
                if (directory && (c == '\\' || c == '/')) continue;
                ret = ret.Replace(c, '_');
            }
            if (directory) ret = @"C:\" + ret[3..];
            return ret.Trim();
        }
        static void UpdatePlaylists()
        {
            foreach (string s in Directory.EnumerateFiles(BASE_DIRECTORY_PATH + PLAYLISTS))
            {
                UpdatePlaylist(s);
            }
        }
        static void UpdatePlaylist(string filename)
        {
            if (!PLAYLIST_FILE_EXTENSIONS.Contains(Path.GetExtension(filename))) return;
            List<string> text = new();
            foreach(string s in System.IO.File.ReadLines(filename))
            {
                if(newFileNames.ContainsKey(s))
                {
                    text.Add(newFileNames[s]);
                } else
                {
                    text.Add(s);
                }
            }
            string toWrite = "";
            foreach(string s in text)
            {
                toWrite += s + "\n";
            }
            System.IO.File.WriteAllText(filename, toWrite);
        }
        static void DeleteEmptyFolders()
        {
            foreach(string s in Directory.EnumerateDirectories(BASE_DIRECTORY_PATH))
            {
                if (IgnoreFolders.Contains(s + @"\")) continue;
                DeleteDirRecursive(s);
            }
        }
        static void DeleteDirRecursive(string path)
        {
            foreach(string s in Directory.EnumerateDirectories(path))
            {
                DeleteDirRecursive(s);
            }
            // https://stackoverflow.com/questions/2811509/c-sharp-remove-all-empty-subdirectories
            if (!Directory.GetFiles(path).Any() && !Directory.GetDirectories(path).Any())
            {
                Directory.Delete(path, false);
            }
            else
            {
                foreach (string s in Directory.GetFiles(path)) Console.WriteLine(s);
            }
        }
    }
}
