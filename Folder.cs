using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace musicsort
{
    public class Folder
    {
        /// <summary>
        /// The path to the folder. Never has a leading backslash, and always has a trailing backslash.
        /// </summary>
        public string DirPath { get; private set; }
        public Folder(string s)
        {
            // https://stackoverflow.com/a/2281769
            DirPath = Path.GetFullPath(s.ToLowerInvariant().TrimStart('\\').TrimEnd('\\') + @"\");
        }
        public override string ToString() => DirPath;
        public static implicit operator Folder(string s) => new(s);
        public static implicit operator string(Folder f) => f.ToString();
        public static Folder operator +(Folder a, Folder b) => new(a.DirPath + b.DirPath);
        public static bool operator ==(Folder a, Folder b) => a.DirPath == b.DirPath;
        public static bool operator !=(Folder a, Folder b) => !(a == b);
    }
}
