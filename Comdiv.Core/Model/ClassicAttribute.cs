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

namespace Comdiv.Model{
    public class ClassicAttribute:Attribute{
        public ClassicAttribute(string name){
            Name = name;
        }
        public string Name { get; set;}
    }

    /// <summary>
    /// Marks interfaces to be found throug total search between distinct databases
    /// </summary>
    public class ForSearchAttribute : Attribute{
        public ForSearchAttribute() {}
        public ForSearchAttribute(string name){
            this.Name = name;
        }

        public static Type[] Collect(params Assembly[] assemblies){
            if(assemblies.no()){
                assemblies =
                    AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.GetName().Name.StartsWith("System")).ToArray();
            }
            var result = new List<Type>();
            foreach (var assembly in assemblies){
                var types =
                    assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof (ForSearchAttribute), false).Length != 0);
                foreach (var type in types){
                    result.Add(type);
                }
            }
            return result.ToArray();
        }
        public string Name { get; set; }

        public static string GetName(Type type){
            var attr =
                type.GetCustomAttributes(typeof (ForSearchAttribute), false).OfType<ForSearchAttribute>().FirstOrDefault
                    ();
            if (null == attr) return type.Name;
            if (attr.Name.noContent()) return type.Name;
            return attr.Name;
        }
    }

    public static class ClassicAttributeExtension{
        public static string GetClassicName(this Type type,bool useClassic){
            ClassicAttribute result = null;
            if (useClassic){
                result = type.getFirstAttribute<ClassicAttribute>();
                
            }
            if (null == result) return type.Name.Substring(1);
            return result.Name;
        }
        public static string GetClassicName(this Type type, string propertyName,bool useClassic){
            ClassicAttribute result = null;
            if (useClassic){
                var property = type.resolveProperty(propertyName);
                result = property.getFirstAttribute<ClassicAttribute>();
            }
            if (null == result) return propertyName;
            return result.Name;
        }
    }
}