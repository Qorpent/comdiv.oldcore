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
    public class OperationLogMap : ExtendedClassMap<OperationLog>{
        public OperationLogMap() : this("comdiv"){
            this.standard();
            Map(x => x.Usr);
            Map(x => x.ObjectType);
            Map(x => x.ObjectCode);
            Map(x => x.OperationType);
            Map(x => x.System);
            Map(x => x.Elapsed);
            Map(x => x.Url);
            Map(x => x.Xml);
        }

        public OperationLogMap(string schema) :
            base(schema) {}
    }
}