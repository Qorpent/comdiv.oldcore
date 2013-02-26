using System.Linq;
using Comdiv.Application;
using Comdiv.Caching;
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
    public abstract class AbstractCacheLease : ICacheLease{
        #region ICacheLease Members

        public abstract bool IsValid { get; }
        public virtual void Retrieved() {}
        public virtual void Refresh() {}

        #endregion
    }
}