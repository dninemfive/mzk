namespace d9.mzk;
public static class Extensions
{
    public static string NullTerminatedStringStartingAt(this byte[] array, uint start)
    {
        string result = string.Empty;
        char cur;
        for (uint i = start; i < array.Length && (cur = (char)array[i]) != '\0'; i++)
            result += cur;
        return result;
    }
}