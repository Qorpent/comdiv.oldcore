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
using FluentNHibernate;

namespace Comdiv.MVC{
    public class LogModel : PersistenceModel{
        public LogModel(){
            Add(new LogItemMap());
            Add(new RoleResolverCacheItemMap());
            Add(new OperationLogMap());
        }
    }
}