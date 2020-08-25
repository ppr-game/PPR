using System.Collections.Generic;
using System.IO;
using System.Text;

using PPR.GUI;
using PPR.Main.Levels;

namespace PPR.Scripts.IO {
    public static class File {
        public static FileStream OpenRead(string path) {
            return System.IO.File.OpenRead(Path.Join("levels", Map.currentLevel.metadata.name, path));
        }
        public static string[] ReadAllLines(string path) {
            return System.IO.File.ReadAllLines(Path.Join("levels", Map.currentLevel.metadata.name, path));
        }
        public static string[] ReadAllLines(string path, Encoding encoding) {
            return System.IO.File.ReadAllLines(Path.Join("levels", Map.currentLevel.metadata.name, path), encoding);
        }
        public static string ReadAllText(string path) {
            return System.IO.File.ReadAllText(Path.Join("levels", Map.currentLevel.metadata.name, path));
        }
        public static string ReadAllText(string path, Encoding encoding) {
            return System.IO.File.ReadAllText(Path.Join("levels", Map.currentLevel.metadata.name, path), encoding);
        }
        public static IEnumerable<string> ReadLines(string path) {
            return System.IO.File.ReadLines(Path.Join("levels", Map.currentLevel.metadata.name, path));
        }
        public static IEnumerable<string> ReadLines(string path, Encoding encoding) {
            return System.IO.File.ReadLines(Path.Join("levels", Map.currentLevel.metadata.name, path), encoding);
        }
    }
}
