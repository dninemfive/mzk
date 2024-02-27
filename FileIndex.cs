using d9.utl;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.mzk;
internal class FileIndex(MediaDevice? device = null)
{
    private MediaDevice? _device = device;
    public MediaDevice? Device
    {
        get
        {
            if (_device is not null && !_device.IsConnected)
                throw new Exception($"FileIndex has non-null device {Device.FriendlyName}, but it is not connected!");
            return _device;
        }
    }
    private OneToOneMap<string, string> _map = new();
    public void Add(string path)
        => _map.TryAddForward(path, Device?.FileHash(path) ?? path.FileHash());
    public string HashOf(string path)
        => _map.TryGetForward(path, out string? result) ? result 
                                                        : throw new Exception($"Path {path} is not indexed on {Device?.FriendlyName.PrintNull()}!");
    public string PathTo(string hash)
        => _map.TryGetBackward(hash, out string? result) ? result
                                                         : throw new Exception($"Hash {hash} is not indexed on {Device?.FriendlyName.PrintNull()}!");
}
