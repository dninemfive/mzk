namespace d9.mzk;
public record FplTrackChunk
{
    public static int ByteCount => 11 * sizeof(uint) + 1 * sizeof(long) + ReplayGainInfo.ByteCount;
    public uint Unknown1, FileOffset, SubsongIndex, FileSize, Unknown2, Unknown3, Unknown4;
    public long Duration;
    public ReplayGainInfo ReplayGain;
    public uint KeyCount, PrimaryKeyCount, SecondaryKeyCount, SecondaryKeyStartOffset;
    public FplTrackChunk(ByteArrayReader bar)
    {
        Unknown1 = bar.ReadNextUint();
        FileOffset = bar.ReadNextUint();
        SubsongIndex = bar.ReadNextUint();
        FileSize = bar.ReadNextUint();
        Unknown2 = bar.ReadNextUint();
        Unknown3 = bar.ReadNextUint();
        Unknown4 = bar.ReadNextUint();
        Duration = bar.ReadNextLong();
        ReplayGain = new(bar.ReadNextInt(), bar.ReadNextInt(), bar.ReadNextInt(), bar.ReadNextInt());
        KeyCount = bar.ReadNextUint();
        PrimaryKeyCount = bar.ReadNextUint();
        SecondaryKeyCount = bar.ReadNextUint();
        SecondaryKeyStartOffset = bar.ReadNextUint();
    }
}