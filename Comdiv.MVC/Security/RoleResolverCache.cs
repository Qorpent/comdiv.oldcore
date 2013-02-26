using System;
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

namespace Comdiv.Security{
    public class RoleResolverCache : ApplicationCacheBase{
        private new static readonly object sync = new object();

        public RoleResolverCache(){
            this.storage = myapp.storage.Get<IRoleResolverCacheItem>();
            SessionKey = Guid.NewGuid().ToString();
            RealType = storage.Resolve();
        }

        private Type RealType { get; set; }

        private string SessionKey { get; set; }
        private IInversionContainer _container;
        private StorageWrapper<IRoleResolverCacheItem> storage;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        protected override void executeBackingStorage(string key, object session){
            lock (sync){
                //using (new ConversationHandler(Conversation).CommitAndReopen()){
                    var res = (bool) session;
                    var result = storage.Load(key) ??
                                 storage.New();

                    result.Name = key;
                    result.Result = res;
                    storage.Save(result);
                    var s = Container.getSession();
                    s.Transaction.Commit();
                    s.BeginTransaction();
               // }
            }
        }


        protected override object getFromBackStorage<TResult>(string key){
            lock (sync){
                using (new TemporarySimpleSession()){
                    var result = storage.Load(key);
                    if (null == result){
                        return null;
                    }
                    return result.Result;
                }
            }
        }
    }
}