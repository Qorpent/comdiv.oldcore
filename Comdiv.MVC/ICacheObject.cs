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
    public interface ICacheObject{
        string Owner { get; set; }
        string Key { get; set; }
        ICacheLease Lease { get; set; }
    }

    public interface ICacheObject<T> : ICacheObject{
        T Target { get; set; }
    }
}