using System;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.MAS {
    [Schema("mas")]
    public class ProcessLog : IWithId, IWithVersion
    {
        public ProcessLog() {
            this.Sender = "sys";
            this.Event = "default";
            this.Message = "";

        }
        public virtual int Id { get; set; }
        public virtual DateTime Version { get; set; }
        [Map]
        public virtual string Sender { get; set; }
        [Map]
        public virtual string Event { get; set; }
        [Map(CustomType = typeof(int))]
        public virtual ProcessLogMessageType Type { get; set; }
        [Map]
        public virtual string Message { get; set; }
        [Map(ReadOnly = true)]
        public virtual int ProcessId { get; set; }
        [Map("ProcessId")]
        public virtual Process Process { get; set; }
    }
}