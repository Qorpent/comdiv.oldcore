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
#region

using System;
using System.Xml.Serialization;
using Comdiv.Extensions;

#endregion

namespace Comdiv.Model.Scaffolding{
    public class PropertyMetadata{
        public static PropertyMetadata Code =
            new PropertyMetadata("Code"){Title = "Код"};

        public static PropertyMetadata Id =
            new PropertyMetadata("Id", typeof (int), true){Title = "Ид"};

        public static PropertyMetadata MyName =
            new PropertyMetadata("Name"){Title = "Название"};

        public bool AsId;

        public bool UseHtml;
        public bool Visible = true;
        public bool Autocomplete;
        public string AutocompleteCustom;
        public string AutocompleteType;

        public string Custom;
        public string CustomRender;
        public string CustomEdit;
        public string CustomNew;
        public string FixedList;

        public bool IsKey;
        public bool IsLookUp;
        public bool IsNullable;
        public string LookupGetDataCall;
        private string customGeter;
        public string CustomGeter{
            get { return customGeter ?? Name; }
            set { customGeter = value; }
        }

        public string CustomSeter
        {
            get; set;
        }

        public string Name;
        public string title;
        [XmlIgnore] private Type type;
        public string TypeName;

        public PropertyMetadata() {}

        public PropertyMetadata(string name){
            Name = name;
            Type = typeof (string);
        }

        public PropertyMetadata(string name, Type type){
            Name = name;
            Type = type;
        }

        public PropertyMetadata(string name, Type type, bool isKey){
            Name = name;
            Type = type;
            IsKey = isKey;
        }

        public PropertyMetadata(string name, Type type, string lookupGetDataCall, bool isNullable){
            Name = name;
            Type = type;
            LookupGetDataCall = lookupGetDataCall;
            IsNullable = isNullable;
            IsLookUp = true;
        }

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
        public string Title{
            get { return title ?? Name; }
            set { title = value; }
        }

        public static PropertyMetadata Create(string name){
            return new PropertyMetadata(name);
        }

        public static PropertyMetadata Create(string name, Type type){
            return new PropertyMetadata(name, type);
        }

        public static PropertyMetadata Create(string name, Type type, bool isKey){
            return new PropertyMetadata(name, type, isKey);
        }

        public static PropertyMetadata Create(string name, Type type, string lookupGetDataCall,
                                              bool isNullable){
            return new PropertyMetadata(name, type, lookupGetDataCall, isNullable);
        }

        public PropertyMetadata SetTitle(string title){
            this.Title = title;
            return this;
        }

        public PropertyMetadata SetGeter(string geter){
            this.CustomGeter = geter;
            return this;
        }
    }
}