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
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.QueryEngine;

namespace Comdiv.Persistence{
    ///<summary>
    ///</summary>
    public class DefaultStorageResolver:IStorageResolver,IWithContainer{
        private IInversionContainer _container;

        public IInversionContainer Container
        {
            get
            {
                if (_container.invalid())
                {
                    lock (this)
                    {
                        if (_container.invalid())
                        {
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        private IList<IStorage> storages;
        public DefaultStorageResolver(){
            UseDefaultIocToInstantiateStorages = true;
        }
        public bool UseDefaultIocToInstantiateStorages
        {
            get;
            set;
        }
        public IList<IStorage> Storages{
            get{
                if(null==storages){
                    lock(this){
                        if(null==storages){
                            if(UseDefaultIocToInstantiateStorages){
                                //load ioc, avoiding recursion
                                storages = Container.all<IStorage>().Where(x=>x!=this).ToList();
                            }else{
                                storages = new List<IStorage>();
                            }
                        }
                    }
                }
                return storages;
            }
        }

        public IEnumerable<IStorage> GetAllStorages(){
            lock (this){
                return Storages.ToArray();
            }
        }

        public StorageWrapper<object> GetDefault(){
            lock (this){
                if (0 == Storages.Count) throw new NotSupportedException("there is no default storage");
                var s = _DefaultStorage();
                return new StorageWrapper<object>(s);
            }
        }

        private IStorage _DefaultStorage(){
            return (IStorage) Storages.OfType<IDefaultStorage>().FirstOrDefault() ?? Storages[0];
        }

        public void Process(StorageQuery query){
            GetDefault().Execute(query);
        }

        public QueryResultCache<StorageQuery, object, StorageQueryContext, StorageQuery> Cache{
            get{
                return _DefaultStorage().Cache;
            }
        }

        public StorageWrapper<T> Get<T>(){
            lock (this){
                return Get<T>(true);
            }
        }

        public StorageWrapper<T> Get<T>(bool throwex){
            return Get<T>("", throwex);
        }

        public StorageWrapper<T> Get<T>(string system, bool throwex){
            lock (this){
                foreach (var storage in Storages){
                    StorageQuery q = null;
                    storage.Process(q = new StorageQuery {System=system, QueryType = StorageQueryType.Supports, TargetType = typeof(T) });
                    if (true.Equals(q.Result))
                    {
                        var result =  new StorageWrapper<T>(storage);
                        result.System = system;
                        return result;
                    }
                }
               if(throwex) throw new NotSupportedException("type "+typeof(T)+" not supported by any storage");
                return null;
            }
        }

        public StorageWrapper<object> Get(Type type){
            lock (this){
                return Get(type, true);
            }
        }

        public StorageWrapper<object> Get(Type type, bool throwex){
            return Get(type, "", throwex);
        }

        public StorageWrapper<object> Get(Type type, string system, bool throwex){
            lock (this){
                foreach (var storage in Storages){
                    StorageQuery q = null;
                    storage.Process(q = new StorageQuery { QueryType = StorageQueryType.Supports, System = system, TargetType = type});
                    if(true.Equals(q.Result)){
                        return new StorageWrapper<object>(storage).WithSystem(system);
                    }
                }
                if(throwex)throw new NotSupportedException("type " + type + " not supported by any storage");
                return null;
            }
        }

        public string DefaultSystem{
            get{
                return _DefaultStorage().DefaultSystem;
            }
            set{
                _DefaultStorage().DefaultSystem = value;
            }
        }

	    public virtual IQueryable<TEntity> AsQueryable<TEntity>(string system = null) {
			foreach (var storage in Storages)
			{
				StorageQuery q = null;
				storage.Process(q = new StorageQuery { QueryType = StorageQueryType.Supports, System = system, TargetType = typeof(TEntity) });
				if (true.Equals(q.Result))
				{
					return new StorageWrapper<object>(storage).WithSystem(system).AsQueryable<TEntity>();
				}
			}
			throw new NotSupportedException("type " + typeof(TEntity) + " not supported by any storage");
	    }
    }
}