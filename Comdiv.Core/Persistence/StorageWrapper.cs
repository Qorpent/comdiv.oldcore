using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.QueryEngine;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Persistence{
    public class StorageWrapper:IStorage{
        protected static object sync;
        protected readonly IList<string> cachedContexts = new List<string>();
        protected readonly IStorage storage;
        static  StorageWrapper(){
            sync = new object();
            ;
        }

        public StorageWrapper(IStorage storage)  {
            this.storage = storage;
        }


        [Overload]
        public R Load<R>(object primaryKey){
            return (R) Load(typeof (R), primaryKey, System, null);
        }


        [Overload]

        public  R First<X, R>( Action<StorageQuery> prepareQuery)
        {
            return Query<X, R>( 1, prepareQuery).FirstOrDefault();
        }


		public virtual IQueryable<TEntity> AsQueryable<TEntity>(string system = null) {
			return this.storage.AsQueryable<TEntity>();
		}
        [Overload]
        public  R First<X, R>( params object[] posparams)
        {
            var result = Query( typeof(X), 0, 1, "", q => { prepareByQueryParams(q,posparams); }).Cast<object>().ToArray();
            if(null==result || result.Length==0 ) {
                return default(R);
            }
            try {
                return result.Cast<R>().FirstOrDefault();
            }catch(NullReferenceException) {
                return default(R);
            }
        }



        [Overload]
        public  object First( Type type, string hql)
        {
            return Query( type, 0, 1, "", q => q.QueryText = hql).Cast<object>().FirstOrDefault();
        }


        [Overload]
        public X First<X>(params object[] objects)
        {
            return Query<X>(1, q=>prepareByQueryParams(q,objects)).FirstOrDefault();
        }


        [Overload]
        public  X First<X>( Action<StorageQuery> prepareQuery)
        {
            return Query<X>( 1, prepareQuery).FirstOrDefault();
        }

       

        

        [Overload]
        public  object First( Type type, Action<StorageQuery> prepareQuery)
        {
            return Query( type, 1, prepareQuery).Cast<object>().FirstOrDefault();
        }






        [Overload]
        
        public  IEnumerable<R> Query<X, R>( Action<StorageQuery> prepareQuery)
        {
            return Query<X, R>( 0, prepareQuery);

        
        }


        [Overload]
        
        public  IEnumerable<X> Query<X>( int count, Action<StorageQuery> prepareQuery)
        {
            return Query<X>( 0, count, prepareQuery);
        }


        [Overload]       
        public  IEnumerable<X> Query<X>( int start, int count,
                                         Action<StorageQuery> prepareQuery)
        {
            return Query<X>( start, count, this.System, prepareQuery);
        }


        [Overload]   
        public  IEnumerable<X> Query<X>( int start, int count, string system,
                                         Action<StorageQuery> prepareQuery)
        {
            var result = Query( typeof(X), start, count, system, prepareQuery);
            return result.Cast<X>().ToArray();
            
        }


        protected void prepareByQueryParams(StorageQuery query,object[] queryparams)
        {
            
            if (queryparams.no())
            {
                return;
            }
            if (queryparams.First() is string)
            {
                query.QueryText = queryparams.First() as string;
                query.QueryTextPositionalParameters = queryparams.Skip(1).ToArray();
            }
            else
            {
                query.CommonQueryObjects = queryparams;
            }
        }

        [Worker]
        
        public  IEnumerable Query( Type type, params object[] parameters)
        {
            return Query( type, q => prepareByQueryParams(q,parameters));
        }

        [Worker]
        
        public  IEnumerable Query( Type type,
                                   Action<StorageQuery> prepareQuery)
        {
            return Query( type, 0, 0, System, prepareQuery);
        }

        [Worker]
        
        public  IEnumerable Query( Type type, int start, int count, string system,
                                   Action<StorageQuery> prepareQuery)
        {
            var query = new StorageQuery
                        {
                            QueryType = StorageQueryType.Query,
                            TargetType = type,
                            System = system,
                            StartIndex = start,
                            MaxCount = count,
                        };
            if (prepareQuery != null)
            {
                prepareQuery(query);
            }
            return Execute<IEnumerable>(query);
        }


        [Overload]
        
        public  IEnumerable<R> Query<X, R>( int count, Action<StorageQuery> prepareQuery)
        {
            return Query<X, R>( 0, count, prepareQuery);
        }

        [Overload]
        
        public  IEnumerable<R> Query<X, R>( int start, int count,
                                            Action<StorageQuery> prepareQuery)
        {
            return Query<X, R>( start, count, System, prepareQuery);
        }

        [Overload]
        
        public  IEnumerable<R> Query<X, R>( int start, int count, string system,
                                            Action<StorageQuery> prepareQuery)
        {
            var result = Query( typeof(X), start, count, system, prepareQuery);
            if (null == result)
            {
                return new R[] { };
            }
            return result.Cast<R>().ToArray();
        }


        [Overload]
        
        public  IEnumerable<X> Query<X>( Action<StorageQuery> prepareQuery)
        {
            return Query<X>( 0, prepareQuery);
        }


        [Overload]
        
        public  IEnumerable<R> Query<X, R>( object objectquery,
                                            params object[] additionalobjects)
        {
            if (objectquery is string)
            {
                return Query<X, R>( QueryDialect.Hql, (string)objectquery, additionalobjects);
            }
            if(objectquery is Type){
                return  Query<X, R>( q =>
                                {
                                         q.TargetType = (Type) objectquery;
                                    q.CommonQueryObjects =
                                        (additionalobjects ?? new object[] { }).ToArray();
                                });
            }
            return Query<X, R>( q =>
                                {
                                    q.CommonQueryObjects =
                                        new[] { objectquery }.Union(additionalobjects ?? new object[] { }).ToArray();
                                });
        }

        [Overload]
        
        public  IEnumerable<R> SqlQuery<X, R>( string textquery,
                                               params object[] positionalParameters)
        {
            return Query<X, R>( q =>
                                {
                                    q.System = System;
                                    q.QueryText = textquery;
                                    q.Dialect = QueryDialect.Sql;
                                    q.QueryTextPositionalParameters = positionalParameters;
                                });
        }


        


        [Overload]
        
        public  IEnumerable<X> Query<X>( object firstParameter,
                                         params object[] commonParameters)
        {
            return Query<X, X>(firstParameter, commonParameters);
        }

        [Overload]
        
        public  IEnumerable<X> Query<X>( QueryDialect dialect, string textquery,
                                         params object[] positionalParameters)
        {
            return Query<X, X>( dialect, textquery, positionalParameters);
        }

        [Overload]
        
        public  IEnumerable<R> Query<X, R>( QueryDialect dialect, string textquery,
                                            params object[] positionalParameters)
        {
            return Query<X, R>( q =>
                                {
                                    q.System = System;
                                    q.Dialect = dialect;
                                    q.QueryText = textquery;
                                    q.QueryTextPositionalParameters = positionalParameters;
                                });
        }


        [Overload]
       
 
        public  Type Resolve<X>()
        {
            return Resolve( typeof(X), System);
        }


        [Overload]
        
        public  Type Resolve<X>( string system)
        {
            return Resolve( typeof(X), system);
        }

        [Worker]
        
        public  Type Resolve( Type type)
        {
            return Resolve( type, System);
        }

        [Worker]
        
        public  Type Resolve( Type type, string system)
        {
            
            var query= new StorageQuery
                       {
                           QueryType = StorageQueryType.Resolve,
                           TargetType = type,
                           System = system
                       };
            
            return Execute<Type>(query);
        }

        [Overload]
        
        public  StorageWrapper Refresh( object target)
        {
            return Refresh( target,System);
        }

        [Worker]
        
        public  StorageWrapper Refresh( object target, string system)
        {
            if (null == target) return this;
            
            var query = new StorageQuery
                        {
                            QueryType = StorageQueryType.Refresh,
                            Target = target,
                            System = system,
                            TargetType = target.GetType(),
                        };
            
            return Execute(query);
        }


        [Overload]
        
        public  IEnumerable<T> All<T>()
        {
            return Query<T>(null);
        }

        

        [Overload]
        
        public  IEnumerable<R> All<T, R>()
        {
            return Query<T, R>( null);
        }


        [Overload]
       
        public  bool Exists<TItem>(object pkOrCode)
        {
            return Exists( typeof(TItem), pkOrCode, System);
        }

        [Overload]
        
        public  bool Exists<TItem>(object pkOrCode, string system)
        {
            return Exists(typeof(TItem), pkOrCode, system);
        }

        [Overload]
        
        public  bool Exists(Type type, object pkOrCode, string system)
        {
           
            var query = new StorageQuery
                        {
                            TargetType = type,
                            QueryType = StorageQueryType.Exists,
                            System = system,
                            Key = pkOrCode,
                        };
            
            return Execute<bool>(query);
        }


        public void Process(StorageQuery query){
            storage.Process(query);
        }

        public QueryResultCache<StorageQuery, object, StorageQueryContext, StorageQuery> Cache{
            get { return storage.Cache; }
        }

        public string DefaultSystem{
            get { return storage.DefaultSystem; }
            set { storage.DefaultSystem = value; }
        }

        public IStorage Storage{
            get { return storage; }
            
        }

        public string System { get; set; }

        public void SetCacheMode(bool useCache){
            lock (sync){
                var prefix = GetThreadPrefix();
                var thekey = prefix + Storage.GetHashCode();
                if (useCache){
                    cachedContexts.Add(thekey);
                }
                else{
                    cachedContexts.Remove(thekey);
                    Storage.Cache.Clear(key => key.StartsWith(prefix));
                }
            }
        }

        [Worker]
        public R Execute<R>(StorageQuery query){
            prepareCache(query);
            Storage.Process(query);

            if (null != query.Error){
                throw new StorageException("query processed with error", query.Error, query);
            }
            if (query.LoadedFromCache){
                query.CommandProcessed = true;
                if (query.Result == null && query.QueryType == StorageQueryType.Supports){
                    query.Result = false;
                }
            }
            if (!query.CommandProcessed){
                if (query.QueryType == StorageQueryType.Resolve){
                    query.Result = null;
                    query.CommandProcessed = true;
                }
                else if (query.QueryType == StorageQueryType.Supports){
                    query.Result = false;
                    query.CommandProcessed = true;
                }
                else{
                    throw new StorageException("command not processed", query);
                }
            }
            return (R) query.Result;
        }

        private string GetThreadPrefix(){
            if (Thread.CurrentThread.Name.noContent()){
                Thread.CurrentThread.Name = Guid.NewGuid().ToString();
            }
            return "__tc_" + Thread.CurrentThread.Name + "_";
        }

        private void prepareCache(StorageQuery query){
            lock (sync){
                var key = GetThreadPrefix() + Storage.GetHashCode();
                if (!cachedContexts.Contains(key)){
                    return;
                }
                if (query.Properties.ContainsKey("cacheKey")){
                    return;
                }
                //HACK: substitute auto-generated code with  name
                if (query.Code.like(@"^\d+$")){
                    query.Code = "__cache_mode_query__";
                }
                query.Properties["cacheKey"] = key + query.GetCacheKey();
                query.Properties["__cacheLease"] = new Func<bool>(() => true);
                query.IsCacheAble = true;
            }
        }

        [Worker]
        [Fluent]
        public StorageWrapper Execute(StorageQuery query){
            prepareCache(query);
            Storage.Process(query);
            if (null != query.Error){
                throw new StorageException("query processed with error", query.Error, query);
            }
            if (!query.CommandProcessed){
                throw new StorageException("command not processed", query);
            }
            return this;
            //}
        }

        [Overload]
        public R New<R>(string system){
            return (R) New(typeof (R), system);
        }

        [Overload]
        public R New<R>(Action<R> ctor){
            var result = (R) New(typeof (R), "");

            if (null != ctor){
                ctor(result);
            }
            return result;
        }

        [Overload]
        public object New(Type type){
            return New(type, System);
        }

        [Worker]
        public object New(Type type, string system){
            var query = new StorageQuery{
                                            QueryType = StorageQueryType.New,
                                            TargetType = type,
                                            System = system
                                        };

            return Execute<object>(query);
        }

        [Overload]
        public R NewOrExisted<R>(string key){
            return (R) NewOrExisted(typeof (R), key);
        }

        [Overload]
        public object NewOrExisted(Type type, string key){
            return NewOrExisted(type, key, System);
        }

        [Overload]
        public R NewOrExisted<R>(string key, string system){
            return (R) NewOrExisted(typeof (R), key, system);
        }

        [Worker]
        public object NewOrExisted(Type type, string key, string system){
            var query = new StorageQuery{
                                            QueryType = StorageQueryType.Load,
                                            TargetType = type,
                                            Key = key,
                                            System = system
                                        };


            var _result = Execute<object>(query);
            if (null != _result){
                return _result;
            }
            return New(type, system);
        }

        [Overload]
        public object Load(Type type, object primaryKey){
            return Load(type, primaryKey, System, null);
        }

        [Overload]
        public object Load(Type type, object primaryKey, string system){
            return Load(type, primaryKey, system, null);
        }

        [Worker]
        public object Load(Type type, object primaryKey, string system,
            Action<StorageQuery> prepare){
                if (0.Equals(primaryKey) || "".Equals(primaryKey)) return null;
            var query = new StorageQuery{
                                            QueryType = StorageQueryType.Load,
                                            TargetType = type,
                                            Key = primaryKey,
                                            System = system
                                        };
            if (prepare != null){
                prepare(query);
            }

            return Execute<object>(query);
        }

        [Overload]
        public R New<R>(){
            return (R) New(typeof (R), System);
        }

        [Overload]
        public bool Supports<R>(){
            return Supports(typeof (R), System);
        }

        [Overload]
        public bool Supports<R>(string system){
            return Supports(typeof (R), system);
        }

        [Overload]
        public bool Supports(Type type){
            return Supports(type, System);
        }

        [Worker]
        public bool Supports(Type type, string system){
            var query = new StorageQuery{
                                            QueryType = StorageQueryType.Supports,
                                            TargetType = type,
                                            System = system
                                        };

            return Execute<bool>(query);
        }

        [Worker]
        [Fluent]
        public StorageWrapper Delete(Type type, object objectToDeleteOrPk,
                                     string system){
            type = type ?? objectToDeleteOrPk.GetType();
            object pk = null;
            var obj = objectToDeleteOrPk;
            if (objectToDeleteOrPk is int || objectToDeleteOrPk is string){
                pk = objectToDeleteOrPk;
                obj = null;
            }

            if (!(objectToDeleteOrPk is string) &&
                objectToDeleteOrPk is IEnumerable){
                var fst =
                    ((IEnumerable) objectToDeleteOrPk).Cast<object>().
                        FirstOrDefault();
                if (null == fst){
                    return this;
                }
                type = fst.GetType();
            }
            if (null == type || type.IsValueType ||
                type.Equals(typeof (string))){
                throw new StorageException(
                    "type of object to delete not choosed", null);
            }
            var query = new StorageQuery{
                                            QueryType =
                                                StorageQueryType.Delete,
                                            TargetType = type,
                                            Target = obj,
                                            Key = pk,
                                            System = system
                                        };

            return Execute(query);
        }

        [Overload]
        [Fluent]
        public StorageWrapper Delete<TItem>(TItem objectToDelete, string system){
            return Delete(typeof (TItem), objectToDelete, system);
        }

        [Overload]
        [Fluent]
        public StorageWrapper Delete<TItem>(TItem objectToDelete){
            return Delete<TItem>(objectToDelete, System);
        }

        [Overload]
        public StorageWrapper Delete<TItem>(int pk, string system){
            return Delete(typeof (TItem), pk, system);
        }

        [Overload]
        public StorageWrapper Delete<TItem>(int pk){
            return Delete(typeof (TItem), pk, System);
        }

        [Overload]
        public StorageWrapper Delete<TItem>(string pk){
            return Delete(typeof (TItem), pk, System);
        }

        [Overload]
        public StorageWrapper Delete<TItem>(string pk, string system){
            return Delete(typeof (TItem), pk, system);
        }

        [Overload]
        [Fluent]
        
        public StorageWrapper Save(object objectToSave)
        {
            return Save( objectToSave, System);
        }

        [Worker]
        [Fluent]
        

        public StorageWrapper Save(object objectToSave, string system)
            
        {
            var query = new StorageQuery
                        {
                            QueryType = StorageQueryType.Save,
                            TargetType = objectToSave.GetType(),
                            Target = objectToSave,
                            System = system
                        };
            
            return Execute(query);
        }

        [Worker]
        public R Define<R>(string code, Action<R> constructor){
            var result = First<R>("Code = ?", code);

            if (BooleanExtensions.no(result)){
                result = New<R>();
                if (result is IWithCode){
                    ((IWithCode) result).Code = code;
                }
                if (constructor.yes()){
                    constructor(result);
                }
                Save(result);
            }


            return result;
        }

        [Overload]
        public R Load<R>(object primaryKey, Action<StorageQuery> prepare){
            return (R) Load(typeof (R), primaryKey, System, prepare);
        }

        [Overload]
        public R Load<R>(object primaryKey, string system){
            return (R) Load(typeof (R), primaryKey, system, null);
        }

        [Overload]
        public R Load<R>(object primaryKey, string system, Action<StorageQuery> prepare){
            return (R) Load(typeof (R), primaryKey, system, prepare);
        }
    }
}