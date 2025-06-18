namespace d9.mzk;
internal class M3U8Playlist(IEnumerable<string> lines) : IPlaylist<M3U8Playlist>
{
    public List<string> Lines { get; private set; } = [.. lines];
    public bool IsCorrectFiletype(string path)
        => Path.GetExtension(path).Equals(".m3u8", StringComparison.OrdinalIgnoreCase);
    public static M3U8Playlist? Read(string path)
        => new(File.ReadAllLines(path).Skip(1));
    public void ChangeRoot(string originalRoot, string newRoot)
    {
        Lines = Lines.Select(x => x.Replace(originalRoot, newRoot).Replace(@"\", "/")).ToList();
    }
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