using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using d9.utl;

namespace d9.mzk
{    
    static class Program
    {
        public static readonly bool DryRun = CommandLineArgs.GetFlag("dryrun", 'D');
        public static Dictionary<string, string> NewFileNames = new();
        public static Log Log { get; private set; }
        static void Main()
        {
            if (CommandLineArgs.GetFlag("resort")) Constants.IgnoreFolders.RemoveAt(0);
            Log = new("mzk.log");
            Log.WriteLine($"mzk running in {(DryRun ? "dry run" : "live")} mode.");
            try
            {
                MoveSongsIn(Constants.BasePath);
                UpdatePlaylists();
                Constants.BasePath.DeleteEmptyFolders(Constants.IgnoreFolders.ToArray());
            }
            finally
            {
                Log.WriteLine("Done!");
                Log.Dispose();
            }
        }
        public static void MoveSongsIn(string folder)
        {            
            foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (ShouldIgnore(file))
                {
                    // Utils.DebugLog($"@ IGNORE: {file}");
                    continue;
                }
                if (Constants.ExtensionsToDelete.Contains(Path.GetExtension(file)))
                {
                    Log.WriteLine($"! DELETE ! {file}");
                    if(!DryRun) File.Delete(file);
                    continue;
                }
                TagUtils.MoveSong(file);
            }
        }        
        static void UpdatePlaylists()
        {
            foreach (string s in Directory.EnumerateFiles(Path.Join(Constants.BasePath, Constants.Playlists))) UpdatePlaylist(s);
        }
        static void UpdatePlaylist(string filename)
        {
            if (!Constants.PlaylistExtensions.Contains(Path.GetExtension(filename))) return;
            List<string> text = new();
            foreach(string s in File.ReadLines(filename))
            {
                if(NewFileNames.TryGetValue(s, out string value))
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
            File.WriteAllText(filename, toWrite);
        }
        public static void MoveToUnsorted(this string oldPath)
        {
            string targetPath = Path.Join(Constants.BasePath, Constants.Unsorted, Path.GetRelativePath(Constants.BasePath, oldPath));
            int ct = 0;
            while (File.Exists(targetPath))
            {
                targetPath = $"{Path.GetFileNameWithoutExtension(targetPath)} ({++ct}){Path.GetExtension(targetPath)}";
            }
            oldPath.MoveFileTo(targetPath);
        }
        public static bool ShouldIgnore(string path)
        {
            return Constants.IgnoreFolders.Where(x => path.IsInFolder(Path.Join(Constants.BasePath, x))).Any();
        }
    }
}
