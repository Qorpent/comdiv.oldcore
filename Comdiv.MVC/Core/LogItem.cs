using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC{
    public class LogItem : ILogItem{
        public virtual Guid Uid { get; set; }

        #region ILogItem Members

        public virtual DateTime Time { get; set; }

        public virtual string Area { get; set; }

        public virtual string Controller { get; set; }

        public virtual string Action { get; set; }

        public virtual string Params { get; set; }

        public virtual string Event { get; set; }

        public virtual string CustomData { get; set; }

        public virtual string Result { get; set; }

        public virtual string Usr { get; set; }

        public virtual DateTime RequestTime { get; set; }

        public virtual int Id { get; set; }

        public virtual DateTime Version { get; set; }

        #endregion

        public override string ToString(){
            return string.Format("{0}/{1}/{2} : {3} => {4} //{5} ({6})", Area, Controller, Action, Event, Result, Params,
                                 Usr);
        }
    }
}