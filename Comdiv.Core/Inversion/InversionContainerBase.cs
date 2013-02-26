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
using System.Linq;
using Comdiv.Extensions;
using Comdiv.Logging;

namespace Comdiv.Inversion{
    ///<summary>
    ///</summary>
    public abstract class InversionContainerBase : IInversionContainer {
        public ILog log = logger.get("comdiv.inversion.container");
        private bool disposed;

        public object Resolve(Type type) {
            return Resolve(type, null);
        }

        public object Resolve(Type type, IDictionary parameters){
            if(log.IsDebugEnabled){
                log.Debug("start resolve "+type.FullName);
            }
            var result = internalResolve(String.Empty, type,parameters);
            if (log.IsDebugEnabled)
            {
                log.Debug("resolved " + type.FullName +" as "+result);
            }
            return result;
        }

        public void Clear(){
            if(log.IsInfoEnabled){
                log.Info("start clear container");
            }
            internalClear();
            if (log.IsInfoEnabled)
            {
                log.Info("end clear container");
            }
        }

        public bool Disposed{
            get { return disposed;}
        }

        public event EventHandler OnDispose;

        protected abstract void internalClear();

        protected abstract object internalResolve(string name, Type serviceType, IDictionary parameters);
        protected abstract IEnumerable internalResolveAll(Type serviceType);

        public object Resolve(string name){
            if (log.IsDebugEnabled)
            {
                log.Debug("start resolve " + name);
            }
            var result = internalResolve(name,null,null);
            if (log.IsDebugEnabled)
            {
                log.Debug("resolved " + name + " as " + result);
            }
            return result;
        }
        public object[] ResolveAll(Type type){
            if (log.IsDebugEnabled)
            {
                log.Debug("start resolve all " + type.FullName);
            }
            var result = internalResolveAll(type).Cast<object>().ToArray();
            if (log.IsDebugEnabled)
            {
                log.Debug("resolved " + type.FullName + " with " + result.Length+" items");
            }
            return result;
        }
        public bool Exists(Type type){
            if(null==type){
                return false;
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("start check exists of " + type.FullName);
            }
            var result = internalCheckExistence(String.Empty, type);
            if (log.IsDebugEnabled)
            {
                log.Debug("end check existence of " + type.FullName + " with " + result);
            }
            return result;
        }

        protected  abstract bool internalCheckExistence(string name, Type type);
        protected abstract void internalRemove(string empty, Type type);

        public bool Exists(string name){
            if(name.noContent()){
                return false;
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("start check exists of " + name);
            }
            var result = internalCheckExistence(name, null);
            if (log.IsDebugEnabled)
            {
                log.Debug("end check existence of " + name + " with " + result);
            }
            return result;
        }

        public IInversionContainer AddTransient(string name, Type type){        
            if (type == null){
                throw new ArgumentNullException("type");
            }
            if (log.IsInfoEnabled)
            {
                log.Info("start add transient with name " + (name ?? String.Empty) + " of type " + type.FullName);
            }
            if (name.yes()){
                Remove(name);
            }else{
                Remove(type);
            }
            internalAdd(name,type,type,LifeStyle.Transient);
            if (log.IsInfoEnabled)
            {
                log.Info("end add transient with name " + (name ?? String.Empty) + " of type " + type.FullName);
            }
            return this;
        }

        public IInversionContainer AddSingleton(string name,Type type,object typeOrInstance){
            
            if (typeOrInstance == null){
                throw new ArgumentNullException("typeOrInstance");
            }
            type = type ?? (typeOrInstance is Type ? (typeOrInstance as Type) : typeOrInstance.GetType());
            if (log.IsInfoEnabled)
            {
                log.Info("start add singleton with name " + (name ?? String.Empty) + " of type " + type.FullName + " implemented by " + typeOrInstance);
            }
            if (name.yes()) Remove(name);

            //HACK: further wee need to avoid such wrong way
            if (name.hasContent() && name.Contains("transient"))
            {
                internalAdd(name, type, typeOrInstance, LifeStyle.Transient);
            }
            else{
                internalAdd(name, type, typeOrInstance, LifeStyle.Singleton);
            }

            if (log.IsInfoEnabled)
            {
                log.Info("end add singleton with name " + (name ?? String.Empty) + " of type " + type.FullName + " implemented by " + typeOrInstance);
            }
            return this;
        }

        protected abstract void internalAdd(string name, Type serviceType, object implementTypeOrInstance, LifeStyle lifeStyle);

        public IInversionContainer Remove(string name){
            if (name.noContent()){
                return this;
            }
            if (log.IsInfoEnabled)
            {
                log.Info("start remove component with name " + name );
            }
            internalRemove(name, null);
            if (log.IsInfoEnabled)
            {
                log.Info("end remove component with name " + name);
            }
            return this;
        }

        public IInversionContainer Remove(Type type){
            if(null==type){
                return this;
            }
            if (log.IsInfoEnabled)
            {
                log.Info("start remove component of type " + type.FullName);
            }
            internalRemove(null,type);
            if (log.IsInfoEnabled)
            {
                log.Info("end remove component of type " + type.FullName);
            }
            return this;
        }


        public void Dispose(){
            disposed = true;
            if(OnDispose!=null){
                OnDispose(this, EventArgs.Empty);
            }
        }
    }
}