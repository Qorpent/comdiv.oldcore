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
using FluentNHibernate.Mapping;

namespace Comdiv.Security.Acl{
    public class AclRuleEntityMap : ClassMap<AclRuleEntity>{
        public AclRuleEntityMap(){
#if LIB2
            Schema("cmdvacl");
#else
            SchemaIs("cmdvacl");
            
#endif

            this.standard();
#if LIB2
            Table("[Rule]");
#else
            WithTable("cmdvacl");
            
#endif
            Map(x => x.TokenMask);
            Map(x => x.PrincipalMask);
            Map(x => x.RuleTypeString);
            Map(x => x.Permissions);
            Map(x => x.System);
            Map(x => x.Active);
            Map(x => x.StartDate);
            Map(x => x.EndDate);
        }
    }
}