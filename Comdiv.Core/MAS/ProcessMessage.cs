using System;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.MAS {
    [Schema("mas")]
    public class ProcessMessage:IWithId,IWithVersion {
        public ProcessMessage() {
            this.Sender = "sys";
            this.Type = "default";
            this.Message = "";
            this.Answer = "";
            this.SendTime = DateTime.Now;
            this.AnswerTime = DateExtensions.End;
        }
        public virtual int Id { get; set; }
        public virtual DateTime Version { get; set; }
        [Map]
        public virtual string Sender { get; set; }
        [Map]
        public virtual int Priority { get; set; }
        [Map]
        public virtual string Type { get; set; }
        [Map]
        public virtual string Message { get; set; }
        [Map]
        public virtual string Answer { get; set; }
        [Map(ReadOnly = true)]
        public virtual int ProcessId { get; set; }
        [Map("ProcessId")]
        public virtual Process Process { get; set; }
        [Map]
        public virtual bool Accepted { get; set; }
        [Map]
        public virtual bool Processed { get; set; }
        [Map]
        public virtual DateTime SendTime { get; set; }
        [Map]
        public virtual DateTime AnswerTime { get; set; }
    }
}