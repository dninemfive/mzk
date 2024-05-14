using System;
using System.Collections.Generic;
using System.IO;
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
        using ByteArrayReader reader = new(File.ReadAllBytes(path));
        byte[] magic = reader.ReadNext(Magic.Length);
        if(magic != Magic)
        {
            Console.WriteLine($"Attempt to read {path} as .fpl failed: magic number did not match!");
            return null;
        }
        uint size = reader.ReadNextUint();
        byte[] data = reader.ReadNext(size);
        uint playlistSize = reader.ReadNextUint();
        while(!reader.ReachedEnd)
        {
            FplTrackChunk curChunk = new(reader);
            FplTrackAttribute[] curAttributes = new FplTrackAttribute[256];
            int attributeCount = (int)(curChunk.PrimaryKeyCount + curChunk.SecondaryKeyCount);
            int realKeys = (int)curChunk.KeyCount - 3;
            uint[] keyArray = reader.ReadNextUints(realKeys);
            int trackIndex = 0;
            for (int i = 0; i < curChunk.PrimaryKeyCount * 2; i += 2)
            {
                uint key = keyArray[i];
                string fieldName = data.NullTerminatedStringStartingAt(keyArray[i + 1]);
                string fieldValue = data.NullTerminatedStringStartingAt(keyArray[keyArray[i] + curChunk.PrimaryKeyCount * 2 + 1]);
                curAttributes[trackIndex] = new(key, fieldName, fieldValue);
            }
        }
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
}