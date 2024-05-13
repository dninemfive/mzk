using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace d9.mzk;
internal interface IPlaylist<TSelf>
{
    public bool IsCorrectFiletype(string path);
    public static abstract TSelf Read(string path);
    public void Update(string oldPath, string newPath);
    public void Write(string path);
}
internal class M3U8Playlist(IEnumerable<string> lines) : IPlaylist<M3U8Playlist>
{
    public List<string> Lines { get; private set; } = [..lines];
    public bool IsCorrectFiletype(string path)
        => Path.GetExtension(path).Equals(".m3u8", StringComparison.OrdinalIgnoreCase);
    public static M3U8Playlist Read(string path)
        => new(File.ReadAllLines(path).Skip(1));
    public void Update(string oldPath, string newPath)
    {
        List<string> lines = new();
        foreach (string s in Lines)
            // todo: better path comparisons
            lines.Add(s == oldPath ? newPath : s);
        Lines = lines;
    }
    public void Write(string path)
        => File.WriteAllText(path, $"#\n{Lines.Aggregate((x, y) => $"{x}\n{y}")}\n");
}
// https://github.com/rr-/fpl_reader/blob/master/fpl-format.md
internal class FplPlaylist : IPlaylist<FplPlaylist>
{
    public readonly byte[] Magic = [ 0xE1, 0xA0, 0x9C, 0x91, 0xF8, 0x3C, 0x77, 0x42, 0x85, 0x2C, 0x3B, 0xCC, 0x14, 0x01, 0xD3, 0xF2 ];
    public int MetaSize, TrackCount;
    [Flags]
    public enum TrackFlags 
    {
        HasMetadata = 0x01,
        Unknown1    = 0x02,
        HasPadding  = 0x04,
        Unknown2    = 0x10,
        Unknown3    = 0x40
    }
    public record TrackEntries(int PrimaryKeyCount,
                               int SecondaryKeyCount,
                               int SecondaryKeysOffset,
                               long[] PrimaryKeys,
                               int[] PrimaryValues,
                               long[] SecondaryKeys)
    {
        
    }
    public record Track(TrackFlags Flags,
                        int FileNameOffset,
                        int SubsongIndex,
                        long FileSize,
                        long FileTime,
                        long Duration,
                        int RpgAlbum,
                        int RpgTrack,
                        int RpkAlbum,
                        int RpkTrack,
                        int EntryCount,
                        byte[] Entries)
    {

    }
    public List<Track> Tracks { get; }
}