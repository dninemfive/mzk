namespace d9.mzk;
public record ReplayGainInfo(int Album, int Track, int AlbumPeak, int TrackPeak)
{
    public static int ByteCount => 4 * sizeof(int);
}