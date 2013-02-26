using System;
using System.IO;

namespace Comdiv.IO {
    public class DirectorySynchronizerTask
    {
        public bool Emulate { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public DirectorySynchronizerCommandType Command { get; set; }
        public void Execute(Action<string> writelog = null)
        {
            writelog = writelog ?? (s => { });

            var logstring = string.Format("{0} {1}->{2} emulate:{3}", this.Command, Source, Target, Emulate);
            writelog(logstring);
            if(!Emulate) {
                switch (Command) {
                    case DirectorySynchronizerCommandType.Delete :
                        executeDelete(writelog);
                        break;
                    default :
                        executeRewrite(writelog);
                        break;
                }
            }

            
        }

        private void executeRewrite(Action<string> writelog) {
            var dirname = Path.GetDirectoryName(Target);
            Directory.CreateDirectory(dirname);
            File.Copy(Source,Target,true);
            writelog("rewrited");
        }

        private void executeDelete(Action<string> writelog) {
            File.Delete(Target);
            writelog("deleted");
        }
    }
}