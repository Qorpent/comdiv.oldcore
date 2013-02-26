using System;

namespace Comdiv.MAS {
    [Flags]
    public enum ProcessLogMessageType {
        None = 0,
        Debug = 1,
        Trace = 2,
        Info = 4,
        Warn = 8,
        Error = 16,
        Fatal = 32,
    }
}