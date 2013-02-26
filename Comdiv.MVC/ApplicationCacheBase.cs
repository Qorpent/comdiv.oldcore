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
using Comdiv.MVC;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Useful;
using Qorpent.Security;

namespace Comdiv.Caching{
    public class ApplicationCacheBase : IApplicationCache{
        protected IList<ICacheObject> cache = new List<ICacheObject>();
        protected object sync = new object();

        private IInversionContainer _container;

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

        public ApplicationCacheBase(){
            CriticalSize = 20;
            NormalSize = 10;
            DefaultLeaseTime = TimeSpan.FromMinutes(2);
        }

        public int CriticalSize { get; set; }
        public int NormalSize { get; set; }
        public IRoleResolver RoleResolver { get; set; }
        public IPrincipalSource PrincipalSource { get; set; }
        public TimeSpan DefaultLeaseTime { get; set; }

        #region IApplicationCache Members

        public string Store<T>(T session, ICacheLease lease){
            lock (sync){
                return Store(GetUniqueKey(session), session, lease);
            }
        }

        public object Get(string key){
            lock (sync){
                return Get<object>(key);
            }
        }

        public object Get(string key, Func<object> realEvaluator){
            lock (sync){
                if (null == cache.FirstOrDefault(i => i.Key == key)){
                    cache.Add(new CacheObject<object>{
                                                         Key = key,
                                                         Lease = GetDefaultLease(),
                                                         Owner = myapp.usrName,
                                                         Target = realEvaluator()
                                                     });
                }
                return cache.First(i => i.Key == key);
            }
        }

        public bool HasKey(string key){
            lock (sync){
                return null != cache.FirstOrDefault(o => o.Key == key);
            }
        }

        public void Remove(string key){
            lock (sync){
                cache.Remove(cache.FirstOrDefault(c => c.Key == key));
                Container.get<IUniqueStringProvider>().Release(new[]{key});
            }
        }

        public void Clear(){
            lock (sync){
                cache.Clear();
            }
        }

        public string Store<T>(string key, T session){
            lock (sync){
                return Store(key, session, GetDefaultLease());
            }
        }


        public virtual string Store<T>(T session){
            return Store(session, GetDefaultLease());
        }

        public virtual void Share(string key){
            var result = cache.OfType<ICacheObject>().FirstOrDefault(c => c.Key.Equals(key));
            if (null == result){
                throw new MvpException("Нет объекта с кодом " + key, null);
            }
            if (!IsAuthorized(result)){
                throw new MvpException("Вы не имеете права делать объект общим", null);
            }
            result.Owner = null;
        }


        public IEnumerable<ICacheObject> All(){
            return cache.copy();
        }


        public virtual T Get<T>(string key){
            lock (sync){
                var result = GetItemByKey<T>(key);
                if (null == result){
                    return default(T);
                }
                if (!IsAuthorized(result)){
                    return default(T);
                }
                //send cached object to top of list
                cache.Remove(result);
                cache.Insert(0, result);
                result.Lease.Retrieved();
                return result.Target;
            }
        }

        public T Ensure<T>(string key, Func<T> creator){
            lock (sync){
                var result = GetItemByKey<T>(key);
                if (null != result){
                    return Get<T>(key);
                }
                var newItem = creator();
                Store(key, newItem);
                return newItem;
            }
        }

        public virtual string Store<T>(string key, T session, ICacheLease lease){
            lock (sync){
                var cacheObject = CreateCacheObject(key, session, lease);
                cache.Insert(0, cacheObject);
                executeBackingStorage(key, session);
                CheckCache();
                return key;
            }
        }

        #endregion

        protected virtual string GetUniqueKey(object session){
            return Container.get<IUniqueStringProvider>().New();
        }

        protected void CheckCache(){
            lock (sync){
                if (cache.Count <= CriticalSize){
                    return;
                }
                //удаляем старые сессии с конца
                foreach (var cacheObject in cache.copy().Reverse()){
                    if (cache.Count >= NormalSize){
                        return;
                    }
                    cacheObject.Lease.Refresh();
                    if (!cacheObject.Lease.IsValid){
                        cache.Remove(cacheObject);
                        Container.get<IUniqueStringProvider>()
                            .Release(new[]{cacheObject.Key});
                    }
                }
            }
        }

        protected virtual bool IsAuthorized(ICacheObject item){
            return true;
        }

        private ICacheObject<T> GetItemByKey<T>(string key){
            var result = cache.OfType<ICacheObject<T>>().FirstOrDefault(c => c.Key.Equals(key));
            if (null == result){
                var backResult = getFromBackStorage<T>(key);
                if (null == backResult){
                    return null;
                }
                Store(key, (T) backResult);
                result = cache.OfType<ICacheObject<T>>().FirstOrDefault(c => c.Key.Equals(key));
            }
            return result;
        }

        protected virtual object getFromBackStorage<TResult>(string key){
            return null;
        }

        protected CacheObject<T> CreateCacheObject<T>(string key, T session, ICacheLease lease){
            return new CacheObject<T>{Key = key, Target = session, Owner = UserName(), Lease = lease};
        }

        private string UserName(){
            if (PrincipalSource == null){
                return null;
            }
            return PrincipalSource.CurrentUser.Identity.Name;
        }


        protected virtual void executeBackingStorage(string key, object session) {}

        protected virtual ICacheLease GetDefaultLease(){
            return new SlidingLease{LeaseTime = DefaultLeaseTime};
        }

        public void Reload(){
            Clear();
        }
    }
}