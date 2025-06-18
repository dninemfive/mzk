using d9.utl;
using MzkLogEntry = (d9.mzk.LogType, string);

namespace d9.mzk.sort;
internal class MusicSorter(string basePath, IEnumerable<string> ignoreFolders, IProgress<MzkLogEntry> log)
{
    public string BasePath => basePath;
    public IEnumerable<string> IgnoreFolders => ignoreFolders;
    IProgress<MzkLogEntry> Log => log;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dryRun"></param>
    /// <returns>A dictionary mapping old paths to new paths.</returns>
    public async Task<IReadOnlyDictionary<string, string>> SortAsync(bool dryRun = true)
    {
        MoveSongsIn(basePath, dryRun);
        BasePath.DeleteEmptyFolders(x => Log.Report((LogType.Delete, x)), [.. IgnoreFolders]);
    }
    public static Task MoveSongsIn(string folder, bool dryRun)
    {
        foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
        {
            if (file.ShouldBeIgnored())
                continue;
            if (file.ShouldDeleteExtension())
            {
                file.DeleteIf(!dryRun);
            }
            else
            {
                TagUtils.MoveSong(file);
            }
        }
    }
    static void UpdatePlaylists()
    {
        string playlistFolder = Path.Join(Constants.BasePath, Constants.Playlists);
        foreach (string s in Directory.EnumerateFiles(playlistFolder))
            UpdatePlaylist(s);
    }
    static void UpdatePlaylist(string filename)
    {
        if (!filename.HasPlaylistExtension())
            return;
        List<string> text = File.ReadLines(filename)
                                .Select(x => NewFileNames.TryGetValue(x, out string? value) ? value : x)
                                .ToList();
        string toWrite = "";
        foreach (string s in text)
            toWrite += $"{s}\n";
        File.WriteAllText(filename, toWrite);
    }
}
