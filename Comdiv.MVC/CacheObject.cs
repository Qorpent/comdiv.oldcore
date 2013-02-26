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

namespace Comdiv.Caching{
    public class CacheObject<T> : ICacheObject<T>{
        #region ICacheObject<T> Members

        public string Owner { get; set; }

        public string Key { get; set; }

        public ICacheLease Lease { get; set; }


        public T Target { get; set; }

        #endregion
    }
}