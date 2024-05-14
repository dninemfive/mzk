using System;
using System.Linq;

namespace d9.mzk;
public class ByteArrayReader(byte[] array) : IDisposable
{
    public byte[] Array = array;
    private int _index = 0;
    public int Index
    {
        get => _index;
        set
        {
            _index = value;
        }
    }
    public byte[] ReadNext(int count)
        => ReadNext((uint)count);
    public byte[] ReadNext(uint count)
    {
        int offset = Index;
        byte[] result = new byte[count];
        for (; Index < count; Index++)
            result[Index - offset] = Array[Index];
        return result;
    }
    public char[] ReadNextChars(uint count)
        => ReadNext(count).Select(x => (char)x).ToArray();
    public uint ReadNextUint()
        => BitConverter.ToUInt32(ReadNext(sizeof(uint)));
    public uint[] ReadNextUints(int count)
    {
        uint[] result = new uint[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadNextUint();
        return result;
    }
    public long ReadNextLong()
        => BitConverter.ToInt64(ReadNext(sizeof(long)));
    public int ReadNextInt()
        => BitConverter.ToInt32(ReadNext(sizeof(int)));
    public char ReadNextChar()
        => BitConverter.ToChar(ReadNext(sizeof(char)));
    public string NextNullTerminatedString()
    {
        string result = string.Empty;
        char c;
        while ((c = ReadNextChar()) != '\0')
            result += c;
        return result;
    }
    public bool ReachedEnd => _index >= Array.Length;
    #region IDisposable
    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
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
