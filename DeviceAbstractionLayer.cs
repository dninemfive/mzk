using d9.utl;
using MediaDevices;

namespace d9.mzk;
public class DeviceAbstractionLayer(MediaDevice? device = null) : IDisposable
{
    public MediaDevice? Device { get; private set; } = device;
    public static DeviceAbstractionLayer? FromName(string name)
    {
        MediaDevice? device = MediaDeviceUtils.ConnectToDeviceWithName(name);
        if (device is not null)
            return new(device);
        return null;
    }
    public string FileHash(string filePath)
        => Device?.FileHash(filePath) ?? filePath.FileHash();
    public IEnumerable<string> EnumerateFilesRecursive(string basePath)
        => Device?.EnumerateFilesRecursive(basePath) ?? basePath.EnumerateFilesRecursive();
    public async Task<IReadOnlyDictionary<string, string>> FileHashCache(string basePath, Progress<string>? progress)
    {
        List<Task<(string path, string hash)>> tasks = new();
        (string path, string hash) generateHashFor(string path)
        {
            string hash = FileHash(path);
            progress.Report(path);
            return (path, hash);
        }
        foreach (string devicePath in EnumerateFilesRecursive(basePath))
            tasks.Add(Task.Run(() => generateHashFor(devicePath)));
        return (await Task.WhenAll(tasks)).ToDictionary();
    }
    #region IDisposable
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Device?.Disconnect();
                Device?.Dispose();
            }
            _disposed = true;
        }
    }
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion IDisposable
}