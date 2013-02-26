using System;

namespace Comdiv.IO.FileSystemScript {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class FileSystemCommandAttribute : Attribute {

        // This is a positional argument
        public FileSystemCommandAttribute(string commandcode) {
            this.CommandCode = commandcode;
        }

        public string CommandCode { get; private set; }

    }
}