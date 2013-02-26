using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.MAS;

namespace Comdiv.IO.FileSystemScript
{
    public interface IFileSystemScriptExecutor {
        void Execute(string scriptcode, TextWriter logwriter = null, IDictionary<string,string > parameters = null);
    }

    public class DefaultScriptExecutor : IFileSystemScriptExecutor {
        public IFilePathResolver Filesystem { get; set; }
        public void Execute(string scriptcode, TextWriter logwriter,IDictionary<string ,string > parameters) {
            Filesystem = Filesystem ?? myapp.files;
            var file = Filesystem.ResolveAll("~/", scriptcode + ".fs.script").FirstOrDefault();
            if(null==file) {
                throw new Exception("cannot find script with code "+scriptcode);
            }
            var script = File.ReadAllText(file);
            var xscript = parseTxt(script);
            var program = new FileSystemProgram();
            if (null != parameters) {
                foreach (var parameter in parameters) {
                    program.Parameters[parameter.Key] = parameter.Value;
                }
            }
            program.Filesystem = this.Filesystem;
            program.Log = new LogHostBase {Writer = logwriter};
            program.Load(xscript);
            program.Execute();
        }

        protected virtual XElement parseTxt(string  script) {
            return XElement.Parse(script);
        }
    }

    public class FileSystemProgram
    {
        public FileSystemProgram()
        {
            Parameters = new Dictionary<string, object>();
            Commands = new List<IFileSystemCommand>();
            Log = ConsoleLogHost.Current ?? new LogHostBase();
            Filesystem = myapp.files;
        }
        public string State { get; set; }
        public int ErrorLevel { get; set; }
        public IDictionary<string, object> Parameters { get; private set; }
        public IList<IFileSystemCommand> Commands { get; private set; }
        public int CurrentCommandIndex { get; set; }
        public IConsoleLogHost Log { get; set; }
        public IFilePathResolver Filesystem { get; set; }

        public void Load(XElement program) {
            this.Commands.Clear();
            this.CurrentCommandIndex = 0;
            var locator = new FileSystemCommandLocator();
            foreach (var element in program.Elements()) {
                var cmd = locator.Get(element);
                if(null==cmd) {
                    Log.logerror("cannot load "+element.ToString());
                    throw new Exception("cannot load " + element);
                }
                cmd.Log = this.Log;
                cmd.Filesystem = this.Filesystem;
                cmd.Programm = this;
                this.Commands.Add(cmd);
            }
        }
        public void Execute() {
            Log.logtrace("program " + this + " started");
            try
            {
                internalExecute();
                Log.logtrace("program " + this + " finished");
            }
            catch (Exception ex)
            {
                Log.logerror("program " + this + " error " + ex.ToString());
                throw;
            }
        }

        protected virtual void internalExecute() {
            while (CurrentCommandIndex<Commands.Count) {
                var cmd = this.Commands[CurrentCommandIndex];
                cmd.Execute();
                CurrentCommandIndex++;
            }
        }
    }
}
