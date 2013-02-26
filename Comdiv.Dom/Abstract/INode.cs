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
using System.Xml.Serialization;
using System.Xml.XPath;
using Comdiv.Extensions;


namespace Comdiv.Dom{
    public interface IStyleCollection : IDictionary<string, string>{
        bool IsSet(string styleName);
        bool AreEqual(string styleName, string value);
        void Apply(string styleString);
    }

    public class StyleCollection : Dictionary<string, string>, IStyleCollection{
        #region IStyleCollection Members

        public bool IsSet(string styleName){
            return ContainsKey(styleName);
        }

        public bool AreEqual(string styleName, string value){
            if (!IsSet(styleName)) return false;
            return this[styleName] == value;
        }

        public void Apply(string styleString){
            if (styleString.hasContent()){
                var styles = styleString.split(false, true, ';');
                foreach (var s in styles){
                    var rule = s.split(false, true, ':');
                    this[rule[0]] = rule[1];
                }
            }
        }

        #endregion
    }

    public interface INode : IXPathNavigable, IXmlSerializable{
        IList<string> Classes { get; }
        IDictionary<string, string> Attributes { get; }
        IStyleCollection Styles { get; }
        IList<INode> ChildrenNodes { get; }
        INode ParentNode { get; set; }
        string Text { get; set; }
        string Name { get; set; }
        string Id { get; set; }
        string Title { get; set; }
        string GetAttribute(string name);
        INode FindById(string id);
    }
}