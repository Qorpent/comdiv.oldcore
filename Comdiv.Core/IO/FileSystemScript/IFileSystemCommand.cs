using System.Collections.Generic;
using Comdiv.MAS;

namespace Comdiv.IO.FileSystemScript {
    public interface IFileSystemCommand {
        string Code { get; set; }
        IDictionary<string, string> Args { get; }
        FileSystemProgram Programm { get; set; }
        IFilePathResolver Filesystem { get; set; }
        IConsoleLogHost Log { get; set; }
        void Execute();
    }
}