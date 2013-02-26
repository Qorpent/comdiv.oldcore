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
using Comdiv.Extensions;

namespace Comdiv.Inversion{
    public class SimpleInversionContainer : InversionContainerBase{
        private readonly List<InversionComponent> components;

        public SimpleInversionContainer(){
            this.components = new List<InversionComponent>();
            this.AddSingleton("this.container", typeof (IInversionContainer), this);
        }

        internal List<InversionComponent> Components{
            get { return components; }
        }

        protected override void internalClear(){
            lock (this){
                foreach (var list in Components){
                    if (list.ImplementationInstance != null){
                        if (list.ImplementationInstance is IDisposable){
                            ((IDisposable) list.ImplementationInstance).Dispose();
                        }
                    }
                }    
                Components.Clear();
            }
            
        }

        protected override object internalResolve(string name, Type serviceType, IDictionary parameters/* will ignore */){
            if(name.yes()){
                return Components.FirstOrDefault(c => c.Name == name).Expand();
            }else{
                return
                    Components.Where(c => c.IsForType(serviceType)).OrderBy(c => c.LifeStyle).
                        FirstOrDefault().Expand();
            }
        }

        protected override IEnumerable internalResolveAll(Type serviceType){
            return Components.Where(c => c.IsForType(serviceType,true)).Select(c => c.Expand());
        }

        protected override bool internalCheckExistence(string name, Type type){
            if(name.yes()){
                return null != Components.FirstOrDefault(c => c.Name == name);
            }
            return null != Components.FirstOrDefault(c => c.IsForType(type));
        }

        protected override void internalRemove(string name, Type type){
            IList<InversionComponent> todelete = new List<InversionComponent>();
            if (name.yes())
            {
                todelete.Add(Components.FirstOrDefault(c => c.Name == name));
            }
            else{
                foreach (var c in Components.Where(c => c.IsForType(type)))
                {
                    todelete.Add(c);
                }
            }
            foreach (var component in todelete){
                Components.Remove(component);
            }
        }

        protected override void internalAdd(string name, Type serviceType, object implementTypeOrInstance, LifeStyle lifeStyle){
            var component = new InversionComponent(this){Name = name};
            serviceType = serviceType ??
                                    (implementTypeOrInstance is Type
                                         ? (implementTypeOrInstance as Type)
                                         : implementTypeOrInstance.GetType());
            implementTypeOrInstance = implementTypeOrInstance ?? serviceType;
            component.ServiceType = serviceType;
            component.ImplementationType = implementTypeOrInstance is Type
                                               ? (implementTypeOrInstance as Type)
                                               : implementTypeOrInstance.GetType();
            component.ImplementationInstance = implementTypeOrInstance is Type ? null : implementTypeOrInstance;
            component.LifeStyle = lifeStyle;
            Components.Add(component);
        }
    }
}