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

namespace Comdiv.Security{
    public class RoleResolverCacheItem : IRoleResolverCacheItem{
        #region IRoleResolverCacheItem Members

        [Map(Title = "Результат")]
        public virtual bool Result { get; set; }

        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        #endregion
    }
}