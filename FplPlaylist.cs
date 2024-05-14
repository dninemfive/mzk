using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.mzk;
// https://github.com/rr-/fpl_reader/blob/master/fpl-format.md
internal class FplPlaylist : IPlaylist<FplPlaylist>
{
    public static readonly byte[] Magic = [ 0xE1, 0xA0, 0x9C, 0x91, 0xF8, 0x3C, 0x77, 0x42, 0x85, 0x2C, 0x3B, 0xCC, 0x14, 0x01, 0xD3, 0xF2 ];

    public static FplPlaylist? Read(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        uint i = 0;
        for(; i < Magic.Length; i++)
        {
            if (bytes[i] != Magic[i])
            {
                Console.WriteLine($"Attempt to read {path} as .fpl failed: magic number did not match!");
                return null;
            }
        }
        byte[] readNext(uint n)
        {
            uint offset = i;
            byte[] result = new byte[n];
            for (; i < n; i++)
                result[i - offset] = bytes[i];
            return result;
        }
        uint readNextUint()
            => BitConverter.ToUInt32(readNext(4));
        uint size = readNextUint();
        byte[] dataPrime = readNext(size);
        uint playlistSize = readNextUint();
    }

    public bool IsCorrectFiletype(string path)
        => Path.GetExtension(path).Equals(".fpl", StringComparison.OrdinalIgnoreCase);
    public void Update(string oldPath, string newPath)
    {
        throw new NotImplementedException();
    }
    public void Write(string path)
    {
        throw new NotImplementedException();
    }
    public record ReplayGainInfo(int Album, int Track, int AlbumPeak, int TrackPeak) { }
    public record FplTrackChunk(uint Unknown1,
                                uint FileOffset,
                                uint SubsongIndex,
                                uint FileSize,
                                uint Unknown2,
                                uint Unknown3,
                                uint Unknown4,
                                long Duration,
                                ReplayGainInfo ReplayGain,
                                uint KeyCount,
                                uint PrimaryKeyCount,
                                uint SecondaryKeyCount,
                                uint SecondaryKeyStartOffset)
    { }
    public record FplTrackAttribute(int Key, byte[] FieldName, byte[] Value) { }
}