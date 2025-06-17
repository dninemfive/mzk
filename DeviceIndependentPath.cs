using MediaDevices;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace d9.mzk;
public readonly struct DeviceIndependentPath(params string[] pathComponents) : IEnumerable<string>
{
    private readonly string[] _components = pathComponents;
    public DeviceIndependentPath? Parent
        => _components.Length > 1 ? new(_components.Skip(1).ToArray()) : null;
    public string ToString(char pathSeparator = '/')
        => _components.Aggregate((x, y) => $"{x}{pathSeparator}{y}");
    public bool ExistsLocally
        => File.Exists(ToString());
    public bool ExistsOnDevice(MediaDevice device)
        => device.FileExists(ToString());
    public bool Exists(MediaDevice? device = null)
        => device is null ? ExistsLocally : ExistsOnDevice(device);
    public static bool operator ==(DeviceIndependentPath a, DeviceIndependentPath b)
        => a._components == b._components;
    public static bool operator !=(DeviceIndependentPath a, DeviceIndependentPath b)
        => !(a == b);
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is DeviceIndependentPath dip && this == dip;
    public override int GetHashCode()
        => HashCode.Combine(_components);
    public IEnumerator<string> GetEnumerator()
    {
        return ((IEnumerable<string>)_components).GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _components.GetEnumerator();
    }
}
