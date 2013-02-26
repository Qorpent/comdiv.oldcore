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

namespace Comdiv.Security.Acl{
    public class AclModel : PersistenceModel{
        public AclModel(){
            Add(new AclRuleEntityMap());
        }
    }
}