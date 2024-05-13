namespace d9.mzk;
internal interface IPlaylist<TSelf>
{
    public bool IsCorrectFiletype(string path);
    public static abstract TSelf? Read(string path);
    public void Update(string oldPath, string newPath);
    public void Write(string path);
}