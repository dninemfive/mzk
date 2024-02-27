using d9.utl;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace d9.mzk;

internal static class Program
{
    public static readonly bool DryRun = CommandLineArgs.GetFlag("dryrun", 'D');
    public static Dictionary<string, string> NewFileNames = new();
    public static Log Log { get; private set; } = new("mzk.log");
    static void Main()
    {
        if (CommandLineArgs.GetFlag("resort")) 
            Constants.IgnoreFolders.RemoveAt(0);
        IEnumerable<string>? copyto = CommandLineArgs.TryGet("copyto", CommandLineArgs.Parsers.Raw);
        Log.WriteLine($"copyto: {copyto?.ListNotation().PrintNull()}");
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
            if (file.ShouldBeIgnored())
                continue;
            if (file.ShouldDeleteExtension())
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
        string playlistFolder = Path.Join(Constants.BasePath, Constants.Playlists);
        foreach (string s in Directory.EnumerateFiles(playlistFolder))
            UpdatePlaylist(s);
    }
    static void UpdatePlaylist(string filename)
    {
        if (!filename.HasPlaylistExtension())
            return;
        List<string> text = File.ReadLines(filename)
                                .Select(x => NewFileNames.TryGetValue(x, out string? value) ? value : x)
                                .ToList();
        string toWrite = "";
        foreach(string s in text)
            toWrite += $"{s}\n";
        File.WriteAllText(filename, toWrite);
    }
    static void MakeDirectoryStructureMatch(string deviceName, string deviceBasePath)
    {
        // for each file in the local path,
        //      if a file exists in the same location and has the same file hash, continue
        //      otherwise,
        //          if a file exists in the destination, delete it
        //          copy file from local to device
        // for each file in the device path,
        //      if no corresponding file exists locally, delete it on the device
        //  delete all empty folders
        // also copy playlists over?
        // and if possible copy playlists from the destination to the source and/or update their file references
    }
}
