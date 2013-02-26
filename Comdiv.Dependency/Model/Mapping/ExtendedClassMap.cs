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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using FluentNHibernate;
using FluentNHibernate.Mapping;
#if LIB2
using FluentNHibernate.MappingModel;
#endif

namespace Comdiv.Model.Mapping{
    public class ExtendedClassMap<T>:ClassMap<T>,IDbSpecified{
        public string Mode { get; set; }
        public ExtendedClassMap()
        {}
        public ExtendedClassMap(string schema)
        {
#if !LIB2
            this.SchemaIs(schema);
#else
           this.Schema(schema);
#endif
            
        }


        public virtual string DbType
        {
            get; set;
        }
#if !LIB2
         public new PropertyMap Map(PropertyInfo info, string columnname){
             return base.Map(info, columnname);
             
             
         }

        public IEnumerable<PropertyMap> GetAllProperties(){

            return this.properties;
        }
#else
        public  PropertyPart Map(PropertyInfo info, string  column) {
            return this.Map(new PropertyMember( info), column);
        }
        public IEnumerable<PropertyMapping> GetAllProperties()
        {

            return this.Properties.Select(x=>x.GetPropertyMapping());
        }
#endif

        //public new ClassMapping GetClassMapping() {
        //    ExecuteConfiguration();
        //    return base.GetClassMapping();
        //}

        public virtual void ExecuteConfiguration() {
            
        }


        //public new HibernateMapping GetHibernateMapping() {
        //     ExecuteConfiguration();
        //    return base.GetHibernateMapping();
        //}
    }
}