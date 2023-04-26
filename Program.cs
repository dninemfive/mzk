using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using d9.utl;
using d9.mzk.compat.TagLib;

namespace d9.mzk
{    
    static class Program
    {
        public static bool DryRun { get; private set; } = false;
        public static Dictionary<string, string> NewFileNames = new();
        public static Log Log { get; private set; }
        static void Main(string[] args)
        {
            if (args.Contains("--dryrun")) DryRun = true;
            if (args.Contains("--resort")) Constants.IgnoreFolders.RemoveAt(0);
            Log = new("mzk.log");
            Log.WriteLine($"Running in {(DryRun ? "dry run" : "live")} mode.");
            try
            {
                MoveSongsIn(Constants.BasePath);
                UpdatePlaylists();
                Constants.BasePath.DeleteEmptyFolders(Constants.IgnoreFolders.ToArray());
            } finally
            {
                Log.WriteLine("Disposing log...");
                Log.Dispose();
            }
        }
        public static void MoveSongsIn(string folder)
        {            
            foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (ShouldIgnore(file))
                {
                    // Utils.WriteLine($"@ IGNORE: {file}");
                    continue;
                }
                if (Constants.ExtensionsToDelete.Contains(Path.GetExtension(file)))
                {
                    Log.WriteLine($"! DELETE ! {file}");
                    if(!DryRun) File.Delete(file);
                    continue;
                }
                TagLibUtils.MoveSong(file);
            }
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
            string targetPath = Constants.BasePath + Constants.Unsorted + oldPath.Replace(Constants.BasePath, "");
            int ct = 0;
            while (File.Exists(targetPath))
            {
                targetPath = $"{Path.GetFileNameWithoutExtension(targetPath)} ({ct}){Path.GetExtension(targetPath)}";
            }
            oldPath.MoveFileTo(targetPath);
        }
        public static bool ShouldIgnore(string path) => Constants.IgnoreFolders.Contains(path);
    }
}
