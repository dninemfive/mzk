using d9.utl;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    static int? FileHash(this string localPath)
        => throw new NotImplementedException();
    static void CopyFileTo(this string localPath, DevicePath devicePath, bool overwrite = false)
        => throw new NotImplementedException();
    static void MakeDirectoryStructureMatchOnDevice(string baseLocalPath, DevicePath baseDevicePath, bool dryRun = true, bool deleteUnmatchedFiles = false)
    {
        HashSet<string> localFiles = Directory.EnumerateFiles(baseLocalPath)
                                              .Select(x => x.RelativeTo(baseLocalPath))
                                              .ToHashSet();
        // for each file in the local path,
        foreach(string localFile in localFiles.Order())
        {
            // if a file exists in the same location and has the same file hash, continue
            if (DevicePath.EquivalentTo(localFile, Constants.BasePath, baseDevicePath.Path) is DevicePath dp && !(dp.FileHash == baseLocalPath.FileHash()))
                // otherwise, overwrite any file at that path with local file
                localFile.CopyFileTo(dp, overwrite: true);
        }
        if(deleteUnmatchedFiles)
        {
            // for each file in the device path,
            foreach (DevicePath devicePath in baseDevicePath.EnumerateFiles())
            {
                if(devicePath.PathRelativeTo(baseDevicePath) is string relativeDevicePath && localFiles.Contains(relativeDevicePath))
                {
                    devicePath.Delete();
                }
            }
        }
        //  delete all empty folders
        // also copy playlists over?
        // and if possible copy playlists from the destination to the source and/or update their file references
    }
}
internal struct DevicePath
{
    public MediaDevice Device;
    public string Path;
    internal int? FileHash
        => throw new NotImplementedException();
    public bool Exists
        => throw new NotImplementedException();
    public bool Delete()
        => throw new NotImplementedException();
    public static DevicePath? EquivalentTo(string localPath, string localBaseFolder, string deviceBaseFolder)
        => throw new NotImplementedException();
    public IEnumerable<DevicePath> EnumerateFiles()
        => throw new NotImplementedException();
    public string? PathRelativeTo(DevicePath other)
        => throw new NotImplementedException();
    public IEnumerable<string> EnumerateRelativePaths()
        => throw new NotImplementedException();
}