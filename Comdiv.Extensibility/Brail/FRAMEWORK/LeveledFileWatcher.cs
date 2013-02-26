using System.IO;

namespace Comdiv.Extensibility.Brail {
    internal class LeveledFileWatcher:FileSystemWatcher {
        public int Level { get; set; }
    }
}