using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPR.Scripts.IO {
    public static class File {
        public static FileStream OpenRead(string path) {
            return System.IO.File.OpenRead(path);
        }
        public static string[] ReadAllLines(string path) {
            return System.IO.File.ReadAllLines(path);
        }
        public static string[] ReadAllLines(string path, Encoding encoding) {
            return System.IO.File.ReadAllLines(path, encoding);
        }
        public static string ReadAllText(string path) {
            return System.IO.File.ReadAllText(path);
        }
        public static string ReadAllText(string path, Encoding encoding) {
            return System.IO.File.ReadAllText(path, encoding);
        }
        public static IEnumerable<string> ReadLines(string path) {
            return System.IO.File.ReadLines(path);
        }
        public static IEnumerable<string> ReadLines(string path, Encoding encoding) {
            return System.IO.File.ReadLines(path, encoding);
        }
    }
}
