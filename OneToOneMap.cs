using MediaDevices;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.mzk;
public class OneToOneMap<TForwardKey, TBackwardKey>
    where TForwardKey : notnull
    where TBackwardKey : notnull
{
    private Dictionary<TForwardKey, TBackwardKey> _forward = new();
    private Dictionary<TBackwardKey, TForwardKey> _backward = new();
    public OneToOneMap() { }
    public bool TryAddForward(TForwardKey t, TBackwardKey u)
        => OneToOneMapUtils.AtomicAdd(_forward, _backward, t, u);
    public bool TryAddBackward(TBackwardKey u, TForwardKey t)
        => OneToOneMapUtils.AtomicAdd(_backward, _forward, u, t);
    public bool TryGetForward(TForwardKey t, [NotNullWhen(true)]out TBackwardKey? result)
        => _forward.TryGetValue(t, out result);
    public bool TryGetBackward(TBackwardKey u, [NotNullWhen(true)]out TForwardKey? result)
        => _backward.TryGetValue(u, out result);
}
internal static class OneToOneMapUtils
{
    internal static bool AtomicAdd<T, U>(Dictionary<T, U> a, Dictionary<U, T> b, T t, U u)
        where T : notnull
        where U : notnull
    {
        if(a.TryAdd(t, u))
        {
            if(b.TryAdd(u, t))
            {
                return true;
            } 
            else
            {
                a.Remove(t);
            }
        }
        return false;
    }
}