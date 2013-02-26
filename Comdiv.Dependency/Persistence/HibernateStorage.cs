// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Comdiv.Application;
using Comdiv.Common;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.QueryEngine;
using NHibernate;
using NHibernate.Linq;
namespace Comdiv.Persistence{

    public class HibernateStorage : IHibernateStorage, IHqlEvaluator
    {

        public HibernateStorage(){
            myapp.OnReload += (s, a) =>
            {
                lock (this){
                    FactoryProvider = null;
                    ResolveTypeMap.Clear();
                    lock (Cache){
                        Cache.Clear();    
                    }
                    
                }
            };
        }

        private IDictionary<string, Type> resolveTypeMap;

        public IDictionary<string, Type> ResolveTypeMap
        {
            [DebuggerStepThrough] //trivial
            get
            {
                lock (this){
                    if (null == resolveTypeMap){
                        resolveTypeMap = new Dictionary<string, Type>();
                    }


                    return resolveTypeMap;
                }
            
            }
        }

        private IInversionContainer _container;
        private ISessionFactoryProvider _factoryProvider;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = ioc.Container;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public ISessionFactoryProvider FactoryProvider
        {
            get
            {
                lock (this)
                {
                    if (null == _factoryProvider)
                    {
                        _factoryProvider = Container.get<ISessionFactoryProvider>();
                    }
                    return _factoryProvider;
                }
            }
            set
            {
                _factoryProvider = value;
            }
        }

        [ThreadStatic] private static HibernateStorageImplementator _current;
        public void Process(StorageQuery query){
            lock (this){
                prepareQuery(query);

                if(query.QueryType==StorageQueryType.Supports){
                    query.CommandProcessed = true;
                    query.Result = query.RealType != null;
                    return;
                    
                }
                if(query.QueryType==StorageQueryType.Resolve){
                    query.CommandProcessed = true;
                    query.Result = query.RealType;
                    return;
                }

                if(query.RealType==null){
                    query.CommandProcessed = false;
                    return;
                    
                }

                if(query.QueryType==StorageQueryType.New){
                    query.Result = query.RealType.create();
                    query.CommandProcessed = true;
                    return;
                }
                
                if(query.IsCacheAble){
                    if(loadFromCache(query)){
                        return;
                    }
                }

                _current = new HibernateStorageImplementator(this, query);
               
            }
            _current.Process();
        }

        private bool loadFromCache(StorageQuery query){
            
            lock (Cache)
            {
                object cachedResult;
                cachedResult = Cache.Get(query);
                if (!Missing.Value.Equals(cachedResult))
                {
                    
                    query.finish(cachedResult);
                    query.LoadedFromCache = true;
                    query.CommandProcessed = true;
                    return true;
                }
                return false;
            }
        }

        private void prepareQuery(StorageQuery query){
            if(string.IsNullOrWhiteSpace(query.System)){
                query.System = this.DefaultSystem;
            }
            if(query.TargetType==null && query.Target!=null){
                query.TargetType = query.Target.GetType();
            }
            if(query.RealType==null && query.TargetType!=null){
                query.RealType = resolveType(query.System,query.TargetType);
            }
            
        }

        private Type resolveType(string system, Type targetType){
            string key = system + targetType.FullName;
            if(ResolveTypeMap.ContainsKey(key)){
                return ResolveTypeMap[key];
            }
            Type result = null;
            var registeredClasses = FactoryProvider.Get(system).GetAllClassMetadata();
                foreach (var pair in registeredClasses){
                    var testType = pair.Value.GetMappedClass(EntityMode.Poco);
                    if (testType.Equals(targetType) || targetType.IsAssignableFrom(testType) || testType.IsAssignableFrom(targetType)){
                        result = testType;
                        break;
                    }
                }
                ResolveTypeMap[key] = result;
            return result;
        }

        private QueryResultCache<StorageQuery, object, StorageQueryContext, StorageQuery> _cache = new QueryResultCache<StorageQuery, object, StorageQueryContext, StorageQuery>();

        public QueryResultCache<StorageQuery, object, StorageQueryContext, StorageQuery> Cache{
            get { return _cache; }
            set { _cache = value; }
        }

        public string DefaultSystem
        {
            get; set;
        }

	    public IQueryable<TEntity> AsQueryable<TEntity>(string system = null) {
		    return FactoryProvider.Get(system).GetCurrentSession().Query<TEntity>();
	    }

	    public IEnumerable Execute(string hql){
            lock(this){
                return FactoryProvider.Get(this.DefaultSystem).GetCurrentSession().CreateQuery(hql).List();
            }
        }
    }
}