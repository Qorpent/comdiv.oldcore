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
using System.Runtime.Serialization;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Comdiv.Extensions;


namespace Comdiv.Inversion{
    public class WindsoreInversionContainer: InversionContainerBase{
        public WindsoreInversionContainer(){
            if(___ThrowErrorOnCreationTo_Express_TypeLoading_Error_ForTesting){
                if(___Exception_to_throw_on_creation_for_testing!=null){
                    throw ___Exception_to_throw_on_creation_for_testing;
                }
                throw new NullReferenceException("type WindsoreInversionContainer was not found");
            }
        }

        internal static bool ___ThrowErrorOnCreationTo_Express_TypeLoading_Error_ForTesting;
        internal static Exception ___Exception_to_throw_on_creation_for_testing;

        
        private IWindsorContainer _container;

        public IWindsorContainer Container{
            get{
                if(_container == null){
                    lock(this){
                        if(_container == null){
                            if(log.IsInfoEnabled){
                                log.Info("start to initialize default windsore container");
                            }
                            _container = new WindsorContainer();
                            _container.Kernel.ComponentRegistered += new ComponentDataDelegate(Kernel_ComponentRegistered);
                           
                         //   _container.LoadDefaultXml();

                            _container.Register(
                                Component.For<IInversionContainer>().LifeStyle.Singleton.Named("this.container").
                                    Instance(this).OverWrite());

                            if (log.IsInfoEnabled)
                            {
                                log.Info("finish to initialize default windsore container");
                            }
                        }
                    }
                }
                return _container;
            }
            private set{
               
                _container = value;
            }
        }

        private IDictionary<string, Type> implementationTypes = new Dictionary<string, Type>();

        void Kernel_ComponentRegistered(string key, IHandler handler)
        {
            implementationTypes[key] = handler.ComponentModel.Implementation;
        }


        protected override void internalClear()
        {
          /*  foreach (var obj in ResolveAll(typeof(object))){
                if(obj is IDisposable){
                    ((IDisposable)obj).Dispose();
                }
            }*/
            Container.Dispose();
            this.Container = new WindsorContainer();
        }

        protected override object internalResolve(string name, Type serviceType, IDictionary parameters){
             try{
            if(name.yes()){
#if LIB2
                return Container.Resolve(name,parameters);
#else
                return Container.Resolve(name);
#endif
            }else{
               
                    return Container.Resolve(serviceType);
                
            }
             }
             catch (ComponentNotFoundException)
             {
                 if (name.hasContent()) throw;
                 //HACK - allow to call SERVICES by it's direct type , services must implement interfaces

                 foreach (var implementationType in implementationTypes)
                 {
                     if (serviceType == implementationType.Value)
                     {
#if LIB2
                         return Container.Resolve(implementationType.Key,parameters);
#else
                         return Container.Resolve(implementationType.Key);
#endif
                     }
                 }


                 return internalResolveAll(serviceType).OfType<object>().FirstOrDefault();
             }
        }

        protected override IEnumerable internalResolveAll(Type serviceType){
            return Container.ResolveAll(serviceType);
        }

        protected override bool internalCheckExistence(string name, Type type){
            if(name.yes()){
                return Container.IsRegistered(name);
            }else{
                return Container.IsRegistered(type);
            }
            
        }

        protected override void internalRemove(string name, Type type){
            if (name.yes())
            {
                Container.Remove(name);
            }
            else
            {
                Container.Remove(type);
            }
        }

        protected override void internalAdd(string name, Type serviceType, object implementTypeOrInstance, LifeStyle lifeStyle){
            if(lifeStyle==LifeStyle.Singleton){
                Container.RegisterService(name,serviceType,implementTypeOrInstance,new Dictionary<string, string>());
            }else{
                Container.RegisterTransient(name,serviceType,implementTypeOrInstance as Type,new Dictionary<string, string>());
            }
        }
    }

  
}