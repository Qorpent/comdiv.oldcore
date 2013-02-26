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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Comdiv.Dom.Const.Xml;
using Comdiv.Extensions;

#endregion

namespace Comdiv.Dom
{

    #region

    #endregion

    #region

    #endregion

    public class Node : INode
    {
        private IDictionary<string, string> attributes;
        private IList<INode> childrenNodes;
        private IList<string> classes;
        private INode parentNode;
        private IStyleCollection styles;
        public bool UseHTMLStyleXml = true;

        public string Class
        {
            get { return 0 == Classes.Count() ? Classes.ToList()[0] : string.Empty; }
            set { Classes.ToList().Add(value); }
        }

        #region INode Members

        public string Name { get; set; }

        public string Id
        {
            get { return GetAttribute("id"); }
            set { Attributes["id"] = value; }
        }

        public string Title
        {
            get { return GetAttribute("title"); }
            set { Attributes["title"] = value; }
        }

        public IList<string> Classes
        {
            get { return classes ?? (classes = new ClassesCollection()); }
        }

        public IStyleCollection Styles
        {
            get { return styles ?? (styles = new StyleCollection()); }
        }

        public IDictionary<string, string> Attributes
        {
            get { return attributes ?? (attributes = new Dictionary<string, string>()); }
        }

        public INode ParentNode
        {
            get { return parentNode; }
            set
            {
                if (value == parentNode) return;
                if (parentNode != null)
                    cleanOldParent();
                parentNode = value;
                prepareNewParent();
            }
        }

        public IList<INode> ChildrenNodes
        {
            get { return childrenNodes ?? (childrenNodes = new NodeCollectionBase<INode>()); }
        }

        public virtual XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public virtual void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            xml_writeStartElement(writer);
            xml_writeAttributes(writer);
            xml_writeClasses(writer);
            xml_writeStyles(writer);
            xml_writeNodes(writer);
            xml_writeInnerPreText(writer);
            xml_writeText(writer);
            xml_writeInnerPostText(writer);
            writer.WriteEndElement();
        }

        public XPathNavigator CreateNavigator()
        {
            var sw = new StringWriter();
            XmlWriter w = XmlWriter.Create(sw);
            WriteXml(w);
            w.Flush();

            var doc = new XPathDocument(new StringReader(sw.ToString()));
            return doc.CreateNavigator();
        }

        public string Text { get; set; }

        public string GetAttribute(string name)
        {
            if (!Attributes.ContainsKey(name)) return null;
            return Attributes[name];
        }

        public INode FindById(string id)
        {
            if (Id == id) return this;
            INode result = null;
            foreach (INode node in ChildrenNodes)
            {
                result = node.FindById(id);
                if (null != result) break;
            }
            return result;
        }

        #endregion

        protected virtual void prepareNewParent()
        {
            if (!ParentNode.ChildrenNodes.Contains(this)) ParentNode.ChildrenNodes.Add(this);
        }

        protected virtual void cleanOldParent()
        {
            ParentNode.ChildrenNodes.Remove((this));
        }

        protected virtual void xml_writeClasses(XmlWriter writer)
        {
            if (0 != Classes.Count)
                writer.WriteAttributeString("class", Classes.Aggregate((f, s) => f + " " + s));
        }

        protected virtual void xml_writeNodes(XmlWriter writer)
        {
            foreach (INode node in ChildrenNodes)
                node.WriteXml(writer);
        }

        protected virtual void xml_writeStyles(XmlWriter writer)
        {
            if (0 != Styles.Count)
            {
                if (UseHTMLStyleXml)
                {
                    var b = new StringBuilder();
                    foreach (var pair in Styles)
                    {
                        b.Append(pair.Key);
                        b.Append(":");
                        b.Append(pair.Value);
                        b.Append(";");
                    }
                    writer.WriteAttributeString("style", string.Empty, b.ToString());
                }
                else
                {
                    foreach (var pair in Styles)
                        writer.WriteAttributeString(pair.Key, Namespaces.DomStylesNamespace, pair.Value);
                }
            }
        }

        protected virtual void xml_writeAttributes(XmlWriter writer)
        {
            foreach (var pair in Attributes)
                writer.WriteAttributeString(pair.Key, pair.Value);
        }

        private void xml_writeText(XmlWriter writer)
        {
            if (Text.hasContent())
                writer.WriteRaw(Text.Replace("&","&amp;"));
        }

        protected virtual void xml_writeStartElement(XmlWriter writer)
        {
            string name = Name;
            if (name.noContent())
                name = GetType().Name;
            writer.WriteStartElement(name, Namespaces.DomNamespace);
        }

        protected virtual void xml_writeInnerPreText(XmlWriter writer)
        {
        }

        protected virtual void xml_writeInnerPostText(XmlWriter writer)
        {
        }

        public int GetAttributeAsInt(string name)
        {
            string value = GetAttribute(name);
            if (value.noContent())
                return 0;
            return value.toInt();
        }

        public bool GetAttributeAsBool(string name)
        {
            return GetAttribute(name).toBool();
        }


        protected void BindClasses(string[] classes, INode node)
        {
            if (null != classes && 0 != classes.Length)
            {
                foreach (string s in classes)
                    node.Classes.Add(s);
            }
        }
    }
}