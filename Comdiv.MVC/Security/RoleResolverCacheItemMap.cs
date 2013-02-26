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

namespace Comdiv.Security{
    public class RoleResolverCacheItemMap : ExtendedClassMap<RoleResolverCacheItem>{
        public RoleResolverCacheItemMap() : this("comdiv") {}

        public RoleResolverCacheItemMap(string schema) : base(schema){
            this.standard();
            
        }
    }
}