using System.IO;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;

namespace Comdiv.Extensibility.Boo.Dsl{
    public class FileBasedBooSourceProvider : IBooSourceProvider{
        private readonly string fileName;
        public FileBasedBooSourceProvider(string fileName) {
            this.fileName = fileName;
        }

        public ICompilerInput GetInput(){
            return new FileInput(FileName);
        }

        public string GetSource(){
            return File.ReadAllText(this.FileName);
        }

        private string FileName{
            get { return fileName; }
        }
    }
}