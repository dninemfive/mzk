﻿using d9.utl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace d9.mzk;

internal static class Program
{
    public static readonly bool DryRun = CommandLineArgs.GetFlag("dryrun", 'D');
    public static Dictionary<string, string> NewFileNames = new();
    static void Main()
    {
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
            // MediaDeviceUtils.PrintStuffAboutMediaDevicesWithNameAndPath(deviceName, devicePath);
            MoveSongsIn(Constants.BasePath);
            UpdatePlaylists();
            Constants.BasePath.DeleteEmptyFolders([.. Constants.IgnoreFolders]);
            if(deviceName is not null && devicePath is not null)
                MediaDeviceUtils.MakeDirectoryStructureMatchOnDevice(Path.Join(Constants.BasePath, "Files"), deviceName, devicePath, DryRun, true);
        }
        finally
        {
            MzkLog.Dispose();
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
}
