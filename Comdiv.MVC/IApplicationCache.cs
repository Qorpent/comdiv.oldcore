using System;
using System.Collections.Generic;
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
    public interface IApplicationCache : ICache<string, object>{
        IEnumerable<ICacheObject> All();
        T Get<T>(string key);
        T Ensure<T>(string key, Func<T> creator);
        string Store<T>(T session);
        string Store<T>(T session, ICacheLease lease);
        string Store<T>(string key, T session);
        string Store<T>(string key, T session, ICacheLease lease);
        void Share(string key);
        bool HasKey(string key);
        void Remove(string key);
        void Clear();
    }
}