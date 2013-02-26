using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;

namespace Comdiv.MAS
{
    [Schema("mas")]
    public class Process:IWithId,IWithVersion,IWithCode,IWithName
    {
        public static Process CreateForCurrentProcess() {
            var result = new Process();
            var currentprocess = System.Diagnostics.Process.GetCurrentProcess();
            result.StartTime = DateTime.Now;
            result.EndTime = DateExtensions.End;
            result.IsActive = true;
            result.Result = -10;
            result.Host = Environment.MachineName;
            result.Pid = currentprocess.Id;
            result.Name = currentprocess.ProcessName;
            result.Usr = Thread.CurrentPrincipal.Identity.Name;
            result.Args = Environment.GetCommandLineArgs().Skip(1).concat(" ").replace(@"\s*--mas-process-code\s*\S+","");
            result.Code = string.Format("{0}_{1}_{2}_{3}_{4}", result.Host, result.Name, result.Pid, result.Usr,result.StartTime.ToString("yyyyMMddHHmmssms")).toSystemName();
            return result;
        }

        public virtual int Id { get; set; }
        public virtual DateTime Version { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual Condition IsActiveCondition { get; set; }
        public virtual Condition ResultCondition { get; set; }
        public virtual Condition StartTimeCondition { get; set; }
        [Map]
        public virtual string Host { get; set; }
        [Map]
        public virtual string Args { get; set; }
        [Map]
        public virtual int Pid { get; set; }
        [Map]
        public virtual string State { get; set; }
        [Map]
        public virtual string Usr { get; set; }
        [Map]
        public virtual bool IsActive { get; set; }
        [Map]
        public virtual int Result { get; set; }
        [Map]
        public virtual string ResultDescription { get; set; }
        [Map]
        public virtual DateTime StartTime { get; set;}
        [Map]
        public virtual DateTime EndTime { get; set; }
        [Map("ProcessId",Cascade = true)]
        public virtual IList<ProcessMessage> Messages { get; set; }
        [Map("ProcessId",Cascade = true)]
        public virtual IList<ProcessLog> Log { get; set; }

        
    }
}
