using System;
using System.Collections.Generic;
using System.Text;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.MAS;

namespace Comdiv.IO.FileSystemScript
{
    public abstract class FileSystemCommand : IFileSystemCommand {
        private IConsoleLogHost log;
        private IFilePathResolver filesystem;

        public string Code { get; set; }
        public FileSystemProgram Programm { get; set; }

        protected FileSystemCommand() {
            this.Args = new Dictionary<string, string>();
            this.log = ConsoleLogHost.Current ?? new LogHostBase();
            this.Filesystem = myapp.files;
        }

        public IDictionary<string, string> Args { get; private set; }

        public IFilePathResolver Filesystem {
            get { return filesystem; }
            set { filesystem = value; }
        }

        public IConsoleLogHost Log {
            get { return log; }
            set { log = value; }
        }

        public void Execute() {
            log.logtrace("command "+this+" started");
            try {
                internalExecute();
                log.logtrace("command " + this + " finished");
            }catch(Exception ex) {
                log.logerror("command " + this + " error "+ ex.ToString());
                throw;
            }
        }

        public override string ToString() {
            var result = Code;
            foreach (var arg in Args) {
                result += " --" + arg.Key + " " + arg.Value.toCmdLineString();
            }
            return result;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(! base.Equals(obj)) return false;
            return obj.ToString().Equals(this.ToString());
        }

        protected abstract void internalExecute();

        protected string setuparg(string val) {
            
            return val.replace(@"\$\{(\w+)\}", m =>
                                                   {
                                                       var code = m.Groups[1].Value;
                                                       if(Programm!=null) {
                                                           return Programm.Parameters.get(code, "");
                                                       }
                                                       return "";
                                                   });
        }
    }
}
