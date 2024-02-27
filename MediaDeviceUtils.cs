using d9.utl;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace d9.mzk;
internal static class MediaDeviceUtils
{
    /// <summary>
    /// Wrapper for <see cref="MediaDevice.FriendlyName"/> which checks that the current operating system
    /// supports looking up the friendly name.
    /// </summary>
    /// <param name="md">The media device whose friendly name to look up.</param>
    /// <returns>The device's friendly name. Throws <see cref="PlatformNotSupportedException"/> if this is not available.</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static string SafeFriendlyName(this MediaDevice md)
    {
        if (!OperatingSystem.IsWindows() || !OperatingSystem.IsWindowsVersionAtLeast(7))
            throw new PlatformNotSupportedException();
        return md.FriendlyName;
    }
    public static IEnumerable<MediaDevice> ConnectedDevicesWithName(string deviceName)
    {
        // suppresses CA1416
        if (!OperatingSystem.IsWindows() || !OperatingSystem.IsWindowsVersionAtLeast(7))
            throw new PlatformNotSupportedException();
        return MediaDevice.GetDevices().Where(x => x.SafeFriendlyName() == deviceName);
    }
    public static void PrintStuffAboutMediaDevicesWithNameAndPath(string? deviceName, string? devicePath)
    {
        if (deviceName is not null && devicePath is not null && OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(7))
        {
            // to suppress CA1416, explicitly declare a getter function
            IEnumerable<MediaDevice> mds = ConnectedDevicesWithName(deviceName);
            MzkLog.WriteLine(mds.Select(SafeFriendlyName).ListNotation());
            foreach (MediaDevice md in mds)
            {
                md.Connect();
                MzkLog.WriteLine(md.PrettyPrint());
                MzkLog.WriteLine(md.EnumerateDirectories(devicePath).ListNotation());
                md.Disconnect();
            }
            foreach (MediaDevice md in mds)
                md.Dispose();
        }
    }
    static void CopyFile(this MediaDevice device, string localPath, string devicePath, bool dryRun = true, bool overwrite = false)
    {
        MzkLog.Move(localPath, devicePath);
        if (!dryRun && (overwrite || !device.FileExists(devicePath)))
            device.UploadFile(localPath, devicePath);
    }
    static void DeleteFile(this MediaDevice device, string devicePath, bool dryRun = true)
    {
        MzkLog.Message(devicePath, LogType.Delete);
        if(!dryRun)
            device.DeleteFile(devicePath);
    }
    static string FileHash(this MediaDevice device, string path)
    {
        using MemoryStream ms = new();
        device.DownloadFile(path, ms);
        string result = ms.FileHash();
        MzkLog.WriteLine($"{device}.FileHash({path}) -> {result}");
        return result;
    }
    static bool FolderIsEmpty(this MediaDevice device, string path)
        => device.DirectoryExists(path) && !device.EnumerateFiles(path).Any() && !device.EnumerateDirectories(path).Any();
    static void DeleteEmptyFolders(this MediaDevice md, string folder, bool dryRun = true, params string[] pathsToIgnore)
    {
        foreach(string path in md.EnumerateDirectories(folder))
        {
            if (pathsToIgnore.Any(folder.IsSubfolderOf))
                md.DeleteFolderRecursive(folder, dryRun);
        }
    }
    static void DeleteFolderRecursive(this MediaDevice md, string folder, bool dryRun = true)
    {
        MzkLog.Message($"(recursive) {folder}", LogType.Delete);
        foreach (string path in md.EnumerateDirectories(folder))
            md.DeleteFolderRecursive(path);
        if(!dryRun && md.FolderIsEmpty(folder))
            md.DeleteDirectory(folder);
    }
    internal static void MakeDirectoryStructureMatchOnDevice(string baseLocalPath, string deviceName, string baseDevicePath, bool dryRun = true, bool deleteUnmatchedFiles = false)
    {
        MzkLog.WriteLine($"MakeDirectoryStructureMatchOnDevice({baseLocalPath}, {deviceName}, {baseDevicePath}, {dryRun}, {deleteUnmatchedFiles})");
        MediaDevice device = ConnectedDevicesWithName(deviceName).First();
        MzkLog.WriteLine($"Connecting to {deviceName}...");
        device.Connect();
        try
        {
            HashSet<string> localFiles = baseLocalPath.EnumerateFilesRecursive()
                                                      .Select(x => x.RelativeTo(baseLocalPath))
                                                      .ToHashSet();
            MzkLog.WriteLine(localFiles.ListNotation());
            Dictionary<string, string> localHashes  = new(),
                                   deviceHashes = new();
            foreach (string relativeFilePath in localFiles.Order())
            {
                string localFilePath  = Path.Join(baseLocalPath, relativeFilePath),
                   deviceFilePath = Path.Join(baseDevicePath, relativeFilePath);
                if (!localHashes.ContainsKey(localFilePath))
                    localHashes[localFilePath] = localFilePath.FileHash();
                if (device.FileExists(deviceFilePath))
                {
                    deviceHashes[deviceFilePath] = device.FileHash(deviceFilePath);
                    if (localHashes[localFilePath] == deviceHashes[deviceFilePath])
                        continue;
                }
                device.CopyFile(localFilePath, deviceFilePath, dryRun, overwrite: true);
            }
            if (deleteUnmatchedFiles)
            {
                // for each file in the device path,
                foreach (string devicePath in device.EnumerateFiles(baseDevicePath))
                {
                    if (devicePath.RelativeTo(baseDevicePath) is string relativePath && localFiles.Contains(relativePath))
                        device.DeleteFile(devicePath, dryRun);
                }
            }
            //  delete all empty folders
            device.DeleteEmptyFolders(baseDevicePath, dryRun);
            // also copy playlists over?
            // and if possible copy playlists from the destination to the source and/or update their file references
        }
        finally
        {
            MzkLog.WriteLine($"Disconnecting from {deviceName}...");
            device.Disconnect();
        }        
    }
}