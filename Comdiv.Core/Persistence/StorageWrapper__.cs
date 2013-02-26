using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Persistence{
    public class StorageWrapper<T> : StorageWrapper{
        public StorageWrapper(IStorage storage):base(storage){
            System = "";    
        }

        public StorageWrapper<T> WithSystem(string system) {
            if(system.noContent()) {
                system = "Default";
            }
            this.System = system;
            return this;
        }

        [Overload]
        public T Load(object primaryKey){
            return (T) Load(typeof (T), primaryKey, System, null);
        }

        [Overload]
        public IEnumerable<T> All(){
            return All<T>();
        }

        [Overload]
        public StorageWrapper Delete(int pk)
        {
            return Delete(typeof(T), pk,System);
        }

        [Overload]
        public StorageWrapper Delete(int pk, string system)
        {
            return Delete(typeof(T), pk, system);
        }

        [Overload]
        public T Define(string code, Action<T> constructor){
            return Define<T>(code, constructor);
        }

        [Overload]
        public T Load(object primaryKey, Action<StorageQuery> prepare){
            return (T) Load(typeof (T), primaryKey, System, prepare);
        }

        [Overload]
        public T Load(object primaryKey, string system){
            return (T) Load(typeof (T), primaryKey, system, null);
        }

        [Overload]
        public T Load(object primaryKey, string system, Action<StorageQuery> prepare){
            return (T) Load(typeof (T), primaryKey, system, prepare);
        }

        [Overload]
        public T New(){
            return (T) New(typeof (T), System);
        }

        [Overload]
        public T New(string system){
            return (T) New(typeof (T), system);
        }

        [Overload]
        public T New(Action<T> ctor){
            return New<T>(ctor);
        }

        [Overload]
        public T NewOrExisted(string key){
            if (null == key) return (T) New(typeof (T));
            return (T) NewOrExisted( typeof (T), key);
        }

        [Overload]
        public T NewOrExisted(string key, string system){
            return (T) NewOrExisted(typeof (T), key, system);
        }

        [Overload]

        public IEnumerable<T> Query(QueryDialect dialect, string textquery,
                                    params object[] positionalParameters)
        {
            return Query<T,T>(dialect, textquery, positionalParameters);
        }

        [Overload]

        public IEnumerable<T> Query(object firstParameter,
                                    params object[] commonParameters)
        {
            
            return Query<T,T>(firstParameter, commonParameters);
        }

        [Overload]

        public IEnumerable<T> Query(Action<StorageQuery> prepareQuery)
        {
            return Query<T>(0, prepareQuery);
        }

        [Overload]
        public IEnumerable<T> Query(int start, int count, string system,
                                    Action<StorageQuery> prepareQuery)
        {
            var result = base.Query(typeof(T), start, count, system, prepareQuery);
            return result.Cast<T>().ToArray();

        }

        [Overload]
        public IEnumerable<T> Query(int start, int count,
                                    Action<StorageQuery> prepareQuery)
        {
            return Query<T>(storage, start, count, String.Empty, prepareQuery);
        }

        [Overload]

        public IEnumerable<T> Query(int count, Action<StorageQuery> prepareQuery)
        {
            return Query<T>( 0, count, prepareQuery);
        }

        [Overload]
        public T First(Action<StorageQuery> prepareQuery)
        {
            return Query<T>(1, prepareQuery).FirstOrDefault();
        }

        [Overload]
        public T First(params object[] objects)
        {
            return Query<T>(1, q => prepareByQueryParams(q, objects)).FirstOrDefault();
        }

        [Overload]

        public bool Exists(object pkOrCode, string system)
        {
            return Exists(typeof(T), pkOrCode, system);
        }

        [Overload]

        public bool Exists(object pkOrCode)
        {
            return Exists(typeof(T), pkOrCode, System);
        }

        [Overload]

        public Type Resolve(string system)
        {
            return Resolve(typeof(T), system);
        }

        [Overload]


        public Type Resolve()
        {
            return Resolve(typeof(T), System);
        }
    }
}