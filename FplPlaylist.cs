using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.mzk;
// https://github.com/rr-/fpl_reader/blob/master/fpl-format.md
internal class FplPlaylist(string path) : IPlaylist<FplPlaylist>
{
    public string Value { get; private set; } = File.ReadAllText(path);
    public static FplPlaylist? Read(string path)
        => new(path);
    public bool IsCorrectFiletype(string path)
        => Path.GetExtension(path).Equals(".fpl", StringComparison.OrdinalIgnoreCase);
    public void Update(string oldPath, string newPath)
        => Value = Value.Replace(oldPath, newPath);
    public void Write(string path)
        => File.WriteAllText(path, Value);
}