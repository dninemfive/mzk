using d9.utl;
using System;

namespace d9.mzk;
internal static class MzkLog
{
    private static Log? Log { get; set; } = new("mzk.log");
    internal const string AlignWithPrefix = "\n           ";
    internal static string Prefix(this LogType logType) => logType switch
    {
        LogType.Delete  =>  "! DELETE !",
        LogType.Error   =>  "!! ERR  !!",
        LogType.Ignore  =>  "@ IGNORE @",
        LogType.Move    =>  ">> MOVE >>",
        LogType.Copy    =>  "+> COPY +>",
        LogType.Warning => @"/!\WARN/!\",
        _ => throw new ArgumentOutOfRangeException(nameof(logType))
    };
    private static Exception LogDisposedException(object msg)
        => new($"{typeof(MzkLog).Name}.{nameof(WriteLine)}({msg.PrintNull().Replace("\n", @"\n")}): Log already disposed!");
    internal static void Write(object msg)
        => (Log ?? throw LogDisposedException(msg)).Write(msg);
    internal static void WriteLine(object msg)
        => (Log ?? throw LogDisposedException(msg)).WriteLine(msg);
    internal static void Message(object msg, LogType logType)
        => WriteLine($"{logType.Prefix()} {msg}");
    internal static void Error(Exception e)
        => Message($"{e.GetType().Name}: {e.Message}", LogType.Error);
    internal static void Error(string msg, Exception e)
        => Message($"{msg}{AlignWithPrefix}{e.GetType().Name}: {e.Message}", LogType.Error);
    internal static void Move(string oldPath, string newPath)
        => Message($" {oldPath}{AlignWithPrefix}↪{newPath}", LogType.Move);
    internal static void Copy(string srcPath, string dstPath)
        => Message($" {srcPath}{AlignWithPrefix}↪{dstPath}", LogType.Copy);
    internal static void Warn(string msg)
        => Message(msg, LogType.Warning);
    internal static void Dispose()
    {
        WriteLine($"Disposing MzkLog...");
        Log?.Dispose();
    }
}
internal enum LogType
{
    Delete,
    Error,
    Ignore,
    Move,
    Copy,
    Warning
}