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
using System.Xml.Serialization;
using Comdiv.Extensions;

namespace Comdiv.Model.Scaffolding{
    public class ClassMetadata{
        [XmlArray]
        public CommandDesc[] Commands;
        public bool Visible = true;
        public string CustomRender;
        public string GroupBy;
        public string Path;
        public bool AllowNew = true;
        public bool AllowEdit = true;
        public bool AllowDelete = true;
        public bool NeedExpansionWithDefaultDescriptor;
        public string Sort { get; set; }
        [XmlIgnore]
        private IDictionary<string,object> parameters = new Dictionary<string, object>();
        [XmlArray] public PropertyMetadata[] Properties;
        public IList<PropertyMetadata> GetVisibleProperties(){
            return Properties.Where(p => p.Visible).ToList();
        }
        public string Title;
        [XmlIgnore] private Type type;
        public string TypeName;

        [XmlIgnore]
        public Type Type{
            get{
                if (null != type) return type;
                if (TypeName.noContent()) return null;
                type = Type.GetType(TypeName);
                return type;
            }
            set{
                type = value;
                TypeName = value.AssemblyQualifiedName;
            }
        }
        [XmlIgnore]
        public IDictionary<string, object> Parameters{
            get { return parameters; }
        }
    }
}