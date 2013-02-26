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
using System.Linq;
using System.Reflection;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Logging;
using Comdiv.Persistence;

namespace Comdiv.Inversion{
    ///<summary>
    ///</summary>
    public static class ioc{
        private static IInversionContainer _container = null;
        readonly static iocFluent Fluent = new iocFluent();
        private static ILog log = logger.get("comdiv.inversion.ioc");
        public static bool PreventFromLoadingDefaultWindsorContainer { get; set; }
        static  object sync = new object();
        public static IInversionContainer Container{
            get{
                if(_container.invalid()){
                    lock(sync){
                        if(_container.invalid()){
                            if(log.IsInfoEnabled){
                                log.Info("start initialize container");
                            }
                            try{
                                if(!PreventFromLoadingDefaultWindsorContainer){
                                    var type =
                                        "Comdiv.Inversion.WindsoreInversionContainer, Comdiv.Dependency".toType();
                                    _container = type.create<IInversionContainer>();
                                } 
                            }
                                catch(NullReferenceException nx){
                                    if (log.IsWarnEnabled)
                                    {
                                        log.Warn("Comdiv.Dependency.dll was not loaded - may be forgotten");
                                    }
                                }
                            catch(TargetInvocationException x){
                                if(x.InnerException is NullReferenceException){
                                    if (log.IsWarnEnabled)
                                    {
                                        log.Warn("Comdiv.Dependency.dll was not loaded - may be forgotten");
                                    }   
                                }else{
                                    throw x.InnerException ?? x;
                                }
                            }
                            
                            if(_container.invalid()){
                                _container = new SimpleInversionContainer();
                            }
                            if (log.IsInfoEnabled)
                            {
                                log.Info("container initialized type: "+_container.GetType().FullName);
                            }
                        }

                    }
                }
                return _container;
            }
            set{
                lock (sync){
                    if (log.IsInfoEnabled)
                    {
                        log.Info("container will changed to: " + value ?? "null");
                    }
                    _container = value;    
                }
                
            }
        }

        public static void finish(){
            lock(sync){
                clear();
                Container.Dispose();
                Container = null;
            }
        }

        public static IEnumerable<T> all<T>()
        {
            return Container.ResolveAll(typeof(T)).Cast<T>();
        }
        public static IEnumerable all(Type type){
            return Container.ResolveAll(type);
        }
        public static T get<T>(string name){
            return get(name).to<T>();
        }
        public static object get(string name){
            if(name.Contains(",")) {
                return name.toType().create();
            }
            return Container.Resolve(name);
        }
        public static T get<T>(){
            return get(typeof (T)).to<T>();
        }
        public static object get(Type type){
            return Container.Resolve(type);
        }

        public static T require<T>(string name)
        {
            return require(name).to<T>();
        }
        public static object require(string name)
        {
            var result = Container.Resolve(name);
            if(null==result){
                throw new InversionException("component with name "+name+" not found");
            }
            return result;
        }
        public static T require<T>()
        {
            return require(typeof(T)).to<T>();
        }
        public static object require(Type type)
        {
            var result = Container.Resolve(type);
            if (null == result)
            {
                throw new InversionException("component with type " + type.FullName + " not found");
            }
            return result;
        }
        [Overload]
        public static iocFluent set<TType>(string name)
        {
            return set(name, typeof(TType));
        }
        public static iocFluent set(string name, Type type)
        {
            Container.AddTransient(name, type);
            return Fluent;
            
        }
[Overload]
        public static IInversionContainer set<T>(this IInversionContainer container, string name){
            return set(container, name, typeof(T));
        }

        [Worker]
        public static IInversionContainer set(this IInversionContainer container, Type type){
            return set(container, 
                null, type);
        }

        [Worker]
        public static IInversionContainer set(this IInversionContainer container, string name, Type type)
        {
            container.AddTransient(name, type);
            return container;

        }
        [Overload]
        public static iocFluent set<SType, IType>(string name)
        {
            return set(name, typeof(SType), typeof(IType));
        }
        [Overload]
        public static iocFluent set<SType>(string name, object typeOrInstance)
        {
            return set(name, typeof(SType), typeOrInstance);
        }
        [Worker]
        public static iocFluent set(string name, Type type, object typeOrInstance)
        {
            Container.AddSingleton(name, type, typeOrInstance);
            return Fluent;
        }



        public static IInversionContainer setAll(this IInversionContainer container, IEnumerable<Type> type)
        {
            foreach (Type t in type)
            {
                container.set(t);
            }
            return container;
        }
        public static IInversionContainer setAll<TAssemblyMarker, TServiceType>(this IInversionContainer container)
        {
            return setAll(container,
                from type in typeof(TAssemblyMarker).Assembly.GetTypes()
                where !type.IsAbstract && typeof(TServiceType).IsAssignableFrom(type)
                select type
                );

        }


        [Overload]
        public static IInversionContainer set<IType>(this IInversionContainer container)
        {
            return set<IType, IType>(container, null);
        }

        [Overload]
        public static IInversionContainer set<SType, IType>(this IInversionContainer container){
            return set<SType, IType>(container, null);
        }

        [Overload]
        public static IInversionContainer set<SType, IType>(this IInversionContainer container, string name)
        {
            return set(container,name, typeof(SType), typeof(IType));
        }
        [Overload]
        public static IInversionContainer set<SType>(this IInversionContainer container,string name, object typeOrInstance)
        {
            return set(container,name, typeof(SType), typeOrInstance);
        }

        

        [Worker]
        public static IInversionContainer set(this IInversionContainer container, string name, Type type, object typeOrInstance)
        {
            
            container.AddSingleton(name, type, typeOrInstance);
            return container;
        }

        public static iocFluent set<TType>()
        {
            return set(typeof(TType));
        }
        public static iocFluent set(Type type)
        {
            return set(String.Empty,type);
        }
      
        public static iocFluent set(string name,object obj)
        {
            if (obj is Type)
            {
                Container.AddTransient(name, (Type)obj);
            }
            else{
                Container.AddSingleton(name, obj.GetType(), obj);
            }
            return Fluent;
        }
        public static iocFluent set(object obj)
        {
            return set(Guid.NewGuid().ToString(), obj);
        }
        public static iocFluent set<SType, IType>() where IType:SType
        {
            return set(typeof(SType), typeof(IType));
        }
        public static iocFluent set<SType>(object typeOrInstance)
        {
            return set(typeof(SType), typeOrInstance);
        }
        public static iocFluent set(Type type, object typeOrInstance)
        {
            return  set(String.Empty,type, typeOrInstance);
        }
        public static iocFluent ensure<TType>(string name)
        {
            return ensure(name, typeof(TType));
        }
        public static iocFluent ensure(string name, Type type)
        {
            bool exists = Container.Exists(name) || Container.Exists(type);
            if (!exists){
                Container.AddTransient(name, type);
            }
            return Fluent;

        }
        public static iocFluent ensure<SType, IType>(string name)
        {
            return ensure(name, typeof(SType), typeof(IType));
        }
        public static iocFluent ensure<SType>(string name, object typeOrInstance)
        {
            return ensure(name, typeof(SType), typeOrInstance);
        }
        public static iocFluent ensure(string name, Type type, object typeOrInstance)
        {
            bool exists = Container.Exists(name) || Container.Exists(type);
            if(!exists && typeOrInstance!=null){
                exists = Container.Exists(typeOrInstance as Type) || Container.Exists(typeOrInstance.GetType());
            }
            if (!exists){
                Container.AddSingleton(name, type, typeOrInstance);
            }
            return Fluent;
        }
        public static iocFluent ensure<TType>()
        {
            return ensure(typeof(TType));
        }
        public static iocFluent ensure(Type type)
        {
            return ensure(String.Empty, type);
        }
        public static iocFluent ensure<SType, IType>()
        {
            return ensure(typeof(SType), typeof(IType));
        }

        

        public static iocFluent ensure<SType>(object typeOrInstance)
        {
            return ensure(typeof(SType), typeOrInstance);
        }
        public static iocFluent ensure(Type type, object typeOrInstance)
        {
            return ensure(String.Empty, type, typeOrInstance);
        }  
        public static iocFluent remove(string name){
            Container.Remove(name);
            return Fluent;
        }
        public static iocFluent remove<T>()
        {
            Container.Remove(typeof(T));
            return Fluent;
        }
        public static iocFluent remove(Type type)
        {
            Container.Remove(type);
            return Fluent;
        }
        public static bool exists(string name){
            return Container.Exists(name);
        }
        public static bool exists(Type type)
        {
            return Container.Exists(type);
        }
        public static bool exists<T>()
        {
            return Container.Exists(typeof(T));
        }

        public static iocFluent clear(){
            Container.Clear();
            return Fluent;
        }

      
    }
}