using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Comdiv.Application;
using Comdiv.ConsoleUtils;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Persistence;

namespace Comdiv.MAS
{
    public abstract class MasConsoleApplication : MasApplicationBase {

        public static MasConsoleApplication Current { get; set; }

        public void Run(string[] args) {
            this.CommandArgs = args;
            Current = this;
            var cargs = new ConsoleArgumentParser().Parse(args);
            foreach (var carg in cargs) {
                this.Args[carg.Key] = carg.Value;
            }
            Run();
        }

        public string[] CommandArgs { get; set; }

        protected override void dispatchMessage(ProcessMessage lastMessage)
        {
            base.dispatchMessage(lastMessage);
            if(!lastMessage.Processed) {
                if(lastMessage.Type=="write") {
                    lastMessage.Processed = true;
                    lastMessage.Answer = "output to log";
                    loginfo(lastMessage.Message, "FROM MESSAGE", lastMessage.Sender);
                }else if(lastMessage.Type=="readline") {
                    lock(consolesync) {
                        Console.Beep(800, 300);
                        Console.Beep(1000, 200);
                        Console.Beep(1500,100);
                        
                        Console.Write(lastMessage.Message+" > ");
                        var result = Console.ReadLine();
                        lastMessage.Answer = result;
                        lastMessage.Processed = true;
                        logtrace("'"+result+"' was sended to "+lastMessage.Sender);
                    }
                }
            }
        } 

        protected override void initialize()
        {
            
            base.initialize();
            this.WriteToLogFile = true;
            if (Args.ContainsKey("--no-writelogfile"))
            {
                this.WriteToLogFile = false;
            }
            if (WriteToLogFile)
            {
                this.LogFile = Args.get("--log-filename", "");
                if (this.LogFile.noContent())
                {
                    var dir = ("mas_agents_temporary_log\\" + this.MasProcess.Name).prepareTemporaryDirectory(false);
                    this.LogFile = Path.Combine(dir, this.MasProcess.Code + ".log");
                    File.WriteAllText(this.LogFile, "");
                }
            }
        }

        protected bool WriteToLogFile { get; set; }

        protected string LogFile { get; set; }
        object consolesync = new object();
        protected override void selflog(ProcessLog processLog) {
            lock (consolesync) {
                var color = getcolor(processLog.Type);
                Console.ForegroundColor = color;
                Console.WriteLine(processLog.Message);
                Console.ResetColor();
                if (this.WriteToLogFile) {
                    using (var sw = new StreamWriter(this.LogFile, true)) {
                        sw.WriteLine("{0}-{1}-{2}", processLog.Type, DateTime.Now, processLog.Message);
                    }
                }
            }
        }

        private ConsoleColor getcolor(ProcessLogMessageType type) {
            switch (type) {
                case ProcessLogMessageType.None:
                    return ConsoleColor.Gray;
                case ProcessLogMessageType.Debug:
                    return ConsoleColor.Gray;
                case ProcessLogMessageType.Trace:
                    return ConsoleColor.White;
                case ProcessLogMessageType.Info:
                    return ConsoleColor.Yellow;
                case ProcessLogMessageType.Warn:
                    return ConsoleColor.Cyan;
                case ProcessLogMessageType.Error:
                    return ConsoleColor.Red;
                case ProcessLogMessageType.Fatal:
                    return ConsoleColor.Magenta;
                default :
                    return ConsoleColor.Gray;     
            }
        }
    }
}
