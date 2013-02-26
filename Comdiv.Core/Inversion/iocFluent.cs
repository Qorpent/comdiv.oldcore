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
using Comdiv.Design;

namespace Comdiv.Inversion{
    ///<summary>
    ///</summary>
    [Fluent]
    public class iocFluent{
        public T get<T>(){
            return ioc.get<T>();
        }
        public iocFluent setAll(IEnumerable<Type> type)
        {
            foreach (Type t in type)
            {
                set(t);
            }
            return this;
        }
        public iocFluent setAll<TAssemblyMarker, TServiceType>()
        {
            return setAll(
                from type in typeof(TAssemblyMarker).Assembly.GetTypes()
                where !type.IsAbstract && typeof(TServiceType).IsAssignableFrom(type)
                select type
                );
            
        }


        public iocFluent set(object obj)
        {
            return ioc.set(obj);
        }
        public iocFluent set<TType>(string name)
        {
            return set(name,typeof(TType));
        }
        public iocFluent set(string name,Type type)
        {
            return ioc.set(name, type);
        }

        public iocFluent set<SType, IType>(string name) where IType:SType
        {
            return set(name,typeof(SType), typeof(IType));
        }
        public iocFluent set<SType>(string name,object typeOrInstance)
        {
            return set(name,typeof(SType), typeOrInstance);
        }
        public iocFluent set(string name,Type type, object typeOrInstance)
        {
            return ioc.set(name, type, typeOrInstance);
        }

        public iocFluent set<TType>(){
            return set(typeof (TType));
        }
        public iocFluent set(Type type){
            return ioc.set(type);
        }
        public iocFluent set<SType,IType>()
        {
            return set(typeof (SType), typeof (IType));
        }
        public iocFluent set<SType>(object typeOrInstance)
        {
            return set(typeof (SType), typeOrInstance);
        }
        public iocFluent set(Type type, object typeOrInstance){
            return ioc.set(type, typeOrInstance);
        }





        public iocFluent ensure<TType>(string name)
        {
            return ensure(name, typeof(TType));
        }
        public iocFluent ensure(string name, Type type)
        {
            return ioc.set(name, type);
        }

        public iocFluent ensure<SType, IType>(string name)
        {
            return ensure(name, typeof(SType), typeof(IType));
        }
        public iocFluent ensure<SType>(string name, object typeOrInstance)
        {
            return ensure(name, typeof(SType), typeOrInstance);
        }
        public iocFluent ensure(string name, Type type, object typeOrInstance)
        {
            return ioc.set(name, type, typeOrInstance);
        }

        public iocFluent ensure<TType>()
        {
            return ensure(typeof(TType));
        }
        public iocFluent ensure(Type type)
        {
            return ioc.set(type);
        }
        public iocFluent ensure<SType, IType>()
        {
            return ensure(typeof(SType), typeof(IType));
        }
        public iocFluent ensure<SType>(object typeOrInstance)
        {
            return ensure(typeof(SType), typeOrInstance);
        }
        public iocFluent ensure(Type type, object typeOrInstance)
        {
            return ioc.set(type, typeOrInstance);
        }

        public  iocFluent remove(string name)
        {
            return ioc.remove(name);
        }
        public  iocFluent remove<T>()
        {
            return ioc.remove<T>();
        }
        public  iocFluent remove(Type type)
        {
            return ioc.remove(type);
        }
        
       
    }
}