using MediaDevices;

namespace d9.mzk;
public static class Extensions
{
    public static void Report<T>(this Progress<T>? progress, T item)
        => (progress as IProgress<T>)?.Report(item);
}
