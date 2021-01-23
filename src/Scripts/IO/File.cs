using System.Collections.Generic;
using System.IO;
using System.Text;

using PPR.Main.Levels;

namespace PPR.Scripts.IO {
    public class File {
        public static FileStream OpenRead(string path) => System.IO.File.OpenRead(Path.Join("levels", Map.currentLevel.metadata.name, path));
        public static string[] ReadAllLines(string path) => System.IO.File.ReadAllLines(Path.Join("levels", Map.currentLevel.metadata.name, path));
        public static string[] ReadAllLines(string path, Encoding encoding) => System.IO.File.ReadAllLines(Path.Join("levels", Map.currentLevel.metadata.name, path), encoding);
        public static string ReadAllText(string path) => System.IO.File.ReadAllText(Path.Join("levels", Map.currentLevel.metadata.name, path));
        public static string ReadAllText(string path, Encoding encoding) => System.IO.File.ReadAllText(Path.Join("levels", Map.currentLevel.metadata.name, path), encoding);
        public static IEnumerable<string> ReadLines(string path) => System.IO.File.ReadLines(Path.Join("levels", Map.currentLevel.metadata.name, path));
        public static IEnumerable<string> ReadLines(string path, Encoding encoding) => System.IO.File.ReadLines(Path.Join("levels", Map.currentLevel.metadata.name, path), encoding);
    }
}
