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
using System.Text;
using Comdiv.Extensions;


namespace Comdiv.Inversion
{
    /// <summary>
    /// allows to define default implementation class for some service interface
    /// main motivation is that in most cases in application interface of service
    /// matched just one main implementation class
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class DefaultImplementationAttribute:Attribute
    {
        /// <summary>
        /// define implementation by name - usefull for mapping to dependen library
        /// </summary>
        /// <param name="typeName">name of implementation type</param>
        public DefaultImplementationAttribute(string typeName){
            this.TypeName = typeName;
        }

       string TypeName { get; set; }

        /// <summary>
        /// define implementation type
        /// </summary>
        /// <param name="type"></param>
        public DefaultImplementationAttribute(Type type){
            this.Type = type;
        }

        private Type _type;
        Type Type{
            get{
                if(null==_type){
                    if(TypeName.hasContent()){
						//on mono it throws exception
						try{
                        _type = System.Type.GetType(TypeName);
						}catch(System.IO.FileNotFoundException){
							throw new DefaultImplementationException("cannot reach type " + TypeName +
                                                                     " may be reference or writing error");
						}
                        if(null==_type){
                            throw new DefaultImplementationException("cannot reach type " + TypeName +
                                                                     " may be reference or writing error");
                        }
                    }
                }
                if(null==_type){
                    throw new DefaultImplementationException("type was not defined");
                }
                return _type;
            }
            set { _type = value; }
        }

        /// <summary>
        /// evaluates type to return
        /// </summary>
        /// <returns>configured implementation type</returns>
        public Type GetImplementationType(){
            return this.Type;
        }
        
    }
    /// <summary>
    /// helper extension to retrieve default implementation by special attribute
    /// </summary>
    public static class DefaultImplementationExtension{

        /// <summary>
        /// returns default implementation for interface types, using DefaultImplementationAttribute
        /// </summary>
        /// <param name="targetType">type of interface to find</param>
        /// <returns>default implementation type</returns>
        /// <exception cref="DefaultImplementationException">on any problem in location and defining implementation type</exception>
        public static Type getDefaultImplementation(this Type targetType)
        {
            if (!targetType.IsInterface)
            {
                throw new DefaultImplementationException("default implementation alowed for interfaces only - " + targetType.FullName + " is not");
            }
            var attributes = targetType.GetCustomAttributes(typeof(DefaultImplementationAttribute), false);
            if (null == attributes || 0 == attributes.Length)
            {
                throw new DefaultImplementationException(targetType.FullName + " has not [DefaultImplementationAttribute] defined");
            }
            var attribute = (DefaultImplementationAttribute)attributes[0];
            var type = attribute.GetImplementationType();
            if (!targetType.IsAssignableFrom(type))
            {
                throw new DefaultImplementationException("provided type " + type.FullName +
                                                         " does not implement " + targetType.Name);
            }
            return type;

        }
    }
}
