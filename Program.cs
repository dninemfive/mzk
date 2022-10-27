using System;
using System.IO;
using TagLib;
using System.Linq;
using System.Collections.Generic;

namespace musicsort
{    
    static class Program
    {
        
        public static Dictionary<string, string> newFileNames = new();        
        static void Main()
        {
            Utils.OpenLog();
            MoveSongsIn(Constants.BasePath);    
            UpdatePlaylists();
            Utils.DeleteEmptyFolders();
            Utils.CloseLog();
        }
        public static void MoveSongsIn(string folder)
        {            
            foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (Utils.ShouldIgnore(file))
                {
                    Utils.WriteLine($"@ IGNORE: {file}");
                    continue;
                }
                if (Constants.ExtensionsToDelete.Contains(Path.GetExtension(file)))
                {
                    Utils.WriteLine($"! DELETE: {file}");
                    //System.IO.File.Delete(file);
                    continue;
                }
                Utils.WriteLine($">  MOVE : {file}");
                // MoveSong(file);
            }
        }
        /// <summary>
        /// Takes a path to a song file and moves it to a location based on its metadata (see NewFilePath() for details)
        /// Done as an atomic operation to ensure playlists are consistent.
        /// </summary>
        /// <param name="oldPath"></param>
        static void MoveSong(string oldPath)
        {
            // load file at path
            Utils.WriteLine($"Moving {oldPath}...");
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(oldPath);
            }
            catch (Exception e)
            {
                oldPath.MoveToUnsorted();
                Utils.WriteLine(e);
                return;
            }
            string newPath = NewPath(file.Tag, oldPath); 
            if (newPath == oldPath) return;
            Utils.WriteLine(newPath);            
            // attempt to move to that path
            if(System.IO.File.Exists(newPath))
            {
                Utils.WriteLine($"Copy from {oldPath} to {newPath} failed (file already exists).");
                oldPath.MoveToUnsorted();
                return;
            }
            try
            {
                oldPath.MoveTo(newPath);
            }
            catch(Exception e)
            {
                Utils.WriteLine($"Copy from {oldPath} to {newPath} failed ({e.Message}).");
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
            // [disc number].[song number] - `
            if (t.Disc != 0)
            {
                newName += t.Disc + "." + t.Track + Constants.NumberSeperator;
            }
            else if (t.Track != 0)
            {
                newName += t.Track + Constants.NumberSeperator;
            }
            // <song name>.<ext>
            newName += t.Title;
            if (newName.Length < 1) return oldName;
            return newName.Safe() + ext;
        }
        public static string NewPath(Tag t, string oldPath) => Path.Join(NewDirectory(t), NewFileName(t, oldPath));
        static string Artist(this Tag t)
        {
            if (!t.JoinedAlbumArtists.NullOrEmpty()) return t.JoinedAlbumArtists;
            if (!t.JoinedPerformers.NullOrEmpty()) return t.JoinedPerformers;
            if (!t.JoinedComposers.NullOrEmpty()) return t.JoinedComposers;
            return "_";
        }
        static string Album(this Tag t)
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
            foreach (char c in Constants.ForbiddenCharacters)
            {
                if (directory && (c == '\\' || c == '/')) continue;
                ret = ret.Replace(c, '_');
            }
            if (directory) ret = @"C:\" + ret[3..];
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
    }
}
