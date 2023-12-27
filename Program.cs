﻿using d9.utl;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace d9.mzk;

static class Program
{
    public static readonly bool DryRun = CommandLineArgs.GetFlag("dryrun", 'D');
    public static Dictionary<string, string> NewFileNames = new();
    public static Log Log { get; private set; }
    static void Main()
    {
        Log = new("mzk.log");
        if (CommandLineArgs.GetFlag("resort")) Constants.IgnoreFolders.RemoveAt(0);
        IEnumerable<string>? copyto = CommandLineArgs.TryGet("copyto", CommandLineArgs.Parsers.Raw);
        Log.WriteLine(copyto?.ListNotation().PrintNull());
        string? deviceName = null, devicePath = null;
        if(copyto is not null)
        {
            if (copyto.Count() < 2)
            {
                Log.WriteLine($"The --copyto command was specified but only {copyto.Count()} arguments were provided. It needs 2!");
                return;
            }
            deviceName = copyto.First();
            devicePath = copyto.ElementAt(1);
        }
        Log.WriteLine($"mzk running in {(DryRun ? "dry run" : "live")} mode.");
        try
        {
            if(deviceName is not null && devicePath is not null && OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(7))
            {
                // to suppress CA1416, explicitly declare a getter function
                static string friendlyName(MediaDevice x)
                {
                    if (!OperatingSystem.IsWindows() || !OperatingSystem.IsWindowsVersionAtLeast(7)) throw new PlatformNotSupportedException();
                    return x.FriendlyName;
                }
                IEnumerable<MediaDevice> mds = MediaDevice.GetDevices().Where(x => friendlyName(x) == deviceName);
                Log.WriteLine(mds.Select(friendlyName).ListNotation());
                foreach(MediaDevice md in mds)
                {
                    md.Connect();
                    Log.WriteLine(md.PrettyPrint());
                    Log.WriteLine(md.EnumerateDirectories(devicePath).PrettyPrint());
                    md.Disconnect();
                }
                foreach (MediaDevice md in mds) md.Dispose();
            }
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
                continue;
            if (Constants.ExtensionsToDelete.Contains(Path.GetExtension(file)))
            {
                file.DeleteIf(!DryRun);
            } 
            else
            {
                TagUtils.MoveSong(file);
            }
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
            text.Add(NewFileNames.TryGetValue(s, out string? value) ? value : s);
        string toWrite = "";
        foreach(string s in text)
            toWrite += $"{s}\n";
        File.WriteAllText(filename, toWrite);
    }
    public static void MoveToUnsorted(this string oldPath)
    {
        string targetPath = Path.Join(Constants.BasePath, Constants.Unsorted, Path.GetRelativePath(Constants.BasePath, oldPath));
        int ct = 0;
        while (File.Exists(targetPath))
            targetPath = $"{Path.GetFileNameWithoutExtension(targetPath)} ({++ct}){Path.GetExtension(targetPath)}";
        oldPath.MoveFileTo(targetPath);
    }
    public static bool ShouldIgnore(string path, bool debugPrint = false)
    {
        bool result = Constants.IgnoreFolders.Any(x => path.IsInFolder(Path.Join(Constants.BasePath, x)));
        if(result && debugPrint)
            Utils.DebugLog($"@ IGNORE: {path}");
        return result;
    }
    static void DeleteIf(this string path, bool delete)
    {
        Log.WriteLine($"! DELETE ! {path}");
        if (delete) File.Delete(path);
    }
}
