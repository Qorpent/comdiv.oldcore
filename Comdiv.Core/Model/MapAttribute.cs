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
using Comdiv.Model.Interfaces;

namespace Comdiv.Model
{
     [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SchemaAttribute : MappingAttribute,IWithName {
        public SchemaAttribute(string name) {
            this.Name = name;
        }

        public string Name { get;  set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MapAttribute : MappingAttribute
    {
        public string Title { get; set; }
        public string ColumnName { get; set; }
        public bool NotNull { get; set; }
        public MapAttribute(string colname = "", bool notnull = false, string type = "", string title = "")
        {
            this.Title = title;
            this.ColumnName = colname;
            this.NotNull = notnull;
        }
        public bool ReadOnly { get; set; }

        public Type CustomType { get; set; }

        public string Formula { get; set; }

        public bool Cascade { get; set; }

    	public bool UseMaxLength { get; set; }

    	public bool NoLazy { get; set; }
    }
    public class NoMapAttribute : MappingAttribute { }
}