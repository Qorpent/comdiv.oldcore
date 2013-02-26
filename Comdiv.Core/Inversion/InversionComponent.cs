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
using System.Reflection;
using Comdiv.Extensions;

namespace Comdiv.Inversion{
    public class InversionComponent{
        public InversionComponent(IInversionContainer container){
            this.container = container;
        }
        internal readonly IInversionContainer container;
        private bool increate = false;
        internal object Create(){
            if(increate)throw new InversionException("circular dependency occured");
            increate = true;
            var instance = ImplementationType.create();
            foreach (var property in ImplementationType.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)){
                //we think that just interfaces and non-collection properties must be wired
                if (null == property.GetSetMethod(true)){
                    continue;
                }
                if(property.PropertyType.IsInterface){
                    if(property.PropertyType.Assembly!=typeof(IEnumerable<object>).Assembly){
                        //HACK: windsore-compatibility without dependency
                        if(null==property.GetCustomAttributes(true).FirstOrDefault(a=>a.GetType().Name=="DoNotWireAttribute")){
                            var value = container.Resolve(property.PropertyType);
                            if(null!=value){
                                property.SetValue(instance,value,null);
                            }
                        }
                    }
                }
            }
            increate = false;
            return instance;
        }
        internal object GetInstance(){
            if (LifeStyle == LifeStyle.Singleton )
            {
                if (ImplementationInstance == null)
                {
                    ImplementationInstance = Create();
                }
                return ImplementationInstance;
            }
            else
            {
                return Create();
            }
        }
        internal string Name { get; set; }
        internal Type ServiceType { get; set; }
        internal Type ImplementationType { get; set; }
        internal object ImplementationInstance { get; set; }
        internal LifeStyle LifeStyle { get; set; }

        internal bool IsForType(Type type){
            return IsForType(type, false);
        }

        internal bool IsForType(Type type, bool alwaysUseServiceType){
            if (LifeStyle == LifeStyle.Singleton || alwaysUseServiceType ){
                if (type.IsAssignableFrom(this.ServiceType)){
                    return true;
                }
            }
            if (ImplementationType.IsAssignableFrom(type)){
                return true;
            }
                
            return false;
        }
            
    }
}