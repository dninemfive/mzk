using d9.utl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace d9.mzk;

internal static class Program
{
    public static readonly bool DryRun = CommandLineArgs.GetFlag("dryrun", 'D');
    public static Dictionary<string, string> NewFileNames = new();
    static async Task Main()
    {
        // M3U8Playlist playlist = M3U8Playlist.Read(Path.Join(Constants.BasePath, Constants.Playlists, "mkondoa.m3u8"))!;
        // playlist.ChangeRoot(Constants.BasePath, "../Music");
        // playlist.Write(Path.Join(Constants.BasePath, Constants.Playlists, "phone", "mkondoa.m3u8"));
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        if (CommandLineArgs.GetFlag("resort")) 
            Constants.IgnoreFolders.RemoveAt(0);
        IEnumerable<string>? copyto = CommandLineArgs.TryGet("copyto", CommandLineArgs.Parsers.Raw);
        MzkLog.WriteLine($"copyto: {copyto?.ListNotation(brackets: null).PrintNull()}");
        string? deviceName = null, devicePath = null;
        if(copyto is not null)
        {
            if (copyto.Count() < 2)
            {
                MzkLog.WriteLine($"The --copyto command was specified but only {copyto.Count()} arguments were provided. It needs 2!");
                return;
            }
            deviceName = copyto.First();
            devicePath = copyto.ElementAt(1);
        }
        MzkLog.WriteLine($"mzk running in {(DryRun ? "dry run" : "live")} mode.");
        try
        {
            MoveSongsIn(Constants.BasePath);
            UpdatePlaylists();
            Constants.BasePath.DeleteEmptyFolders([.. Constants.IgnoreFolders]);
            if(deviceName is not null && devicePath is not null)
                await MediaDeviceUtils.MakeDirectoryStructureMatchOnDevice(Path.Join(Constants.BasePath, "Files"), deviceName, devicePath, DryRun, false);
        }
        finally
        {
            MzkLog.Dispose();
        }
    }     
}
