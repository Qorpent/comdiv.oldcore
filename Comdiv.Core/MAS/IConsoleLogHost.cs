using System;
using System.IO;

namespace Comdiv.MAS {
    public interface IConsoleLogHost {
        void logdebug(string message, string eventtype = "default", string  source = "self");
        void logtrace(string message, string eventtype = "default", string  source = "self");
        void loginfo(string message, string eventtype = "default", string  source = "self");
        void logwarn(string message, string eventtype = "default", string  source = "self");
        void logerror(string message, string eventtype = "default", string  source = "self");
        void logfatal(string message, string eventtype = "default", string  source = "self");
        ProcessLog log(string message, string eventtype = "default", string  source = "self", ProcessLogMessageType type = ProcessLogMessageType.Debug, bool onlyself = false);
    }

    public class LogHostBase : IConsoleLogHost {
        public void logdebug(string message, string eventtype = "default", string source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Debug);
        }

        public void logtrace(string message, string eventtype = "default", string source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Trace);
        }

        public void loginfo(string message, string eventtype = "default", string source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Info);
        }

        public void logwarn(string message, string eventtype = "default", string source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Warn);
        }

        public void logerror(string message, string eventtype = "default", string source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Error);
        }

        public void logfatal(string message, string eventtype = "default", string source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Fatal);
        }



        public TextWriter Writer { get; set; }

        public ProcessLog log(string message, string eventtype = "default", string source = "self", ProcessLogMessageType type = ProcessLogMessageType.Debug, bool onlyself = false)
        {
            if(Writer!=null) {
                Writer.WriteLine("{0} {1} {2} {3} {4}",DateTime.Now,source,type,eventtype,message);
            }
            return null;
        }
    }

    

    public static class ConsoleLogHost {
        private static IConsoleLogHost _current = new LogHostBase();
        public static IConsoleLogHost Current {
            get { return _current ; }
            set { _current  = value; }
        }
    }
}