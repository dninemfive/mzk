using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace musicsort
{
    public static class Utils
    {
        public static void DeleteEmptyFolders()
        {
            foreach (string s in Directory.EnumerateDirectories(Constants.BasePath))
            {
                if (Constants.IgnoreFolders.Contains(s + @"\")) continue;
                DeleteDirRecursive(s);
            }
        }
        public static void DeleteDirRecursive(string path)
        {
            foreach (string s in Directory.EnumerateDirectories(path))
            {
                DeleteDirRecursive(s);
            }
            // https://stackoverflow.com/questions/2811509/c-sharp-remove-all-empty-subdirectories
            if (!Directory.GetFiles(path).Any() && !Directory.GetDirectories(path).Any())
            {
                Directory.Delete(path, false);
            }
            else
            {
                foreach (string s in Directory.GetFiles(path)) Console.WriteLine(s);
            }
        }
        public static void CopyTo(this string oldPath, string newPath)
        {
            if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);
            File.Copy(oldPath, newPath);
        }
        public static void MoveTo(this string oldPath, string newPath)
        {
            oldPath.CopyTo(newPath);
            File.Delete(oldPath);
        }
        public static void MoveToUnsorted(this string oldPath)
        {
            string targetPath = Constants.BasePath + Constants.Unsorted + oldPath.Replace(Constants.BasePath, "");
            int ct = 0;
            while (File.Exists(targetPath))
            {
                targetPath = $"{Path.GetFileNameWithoutExtension(targetPath)} ({ct}){Path.GetExtension(targetPath)}";
            }
            oldPath.MoveTo(targetPath);
        }
    }
}
