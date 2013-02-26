using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Mapping;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC{
    public class LogItemMap : ExtendedClassMap<LogItem>{
        public LogItemMap() : this("dbo") {}

        public LogItemMap(string schema) : base(schema){
            #if LIB2
            Table("Log");
            #else
            WithTable("Log");
            #endif
            this.standard();
            Map(x => x.Time);
            Map(x => x.Area);
            Map(x => x.Controller);
            Map(x => x.Action);
            Map(x => x.Params);
            Map(x => x.Event);
            Map(x => x.CustomData);
            Map(x => x.Result);
            Map(x => x.Usr);
            Map(x => x.RequestTime);
        }
    }
}