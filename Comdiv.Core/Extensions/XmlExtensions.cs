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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Persistence;

namespace Comdiv.Extensions{
    
    
    ///<summary>
    ///</summary>
    /// 
    
    public static class XmlExtensions{
        public static IEnumerable<XElement> withAttribute(this IEnumerable<XElement> elements, string name, string value)
        {
            return withAttribute(elements, name, x => value == x.Value);
        }

        public static string idorvalue(this XElement xml)
        {
            string x;
            return (x = xml.attr("id")).no() ? xml.Value : x;
        }

        public static XElement merge(this IEnumerable<XElement> elements, string rootname = "root")
        {
            rootname = rootname ?? "root";
            var root = new XElement(rootname);
            foreach (var e in elements)
            {
                foreach (var a   in e.Attributes())
                {
                    root.SetAttributeValue(a.Name,a.Value);
                }
                foreach (var e_ in e.Elements())
                {
                   root.Add(e_); 
                }
            }

            return root;
        }


        public static XElement processIncludes(this XElement xElement, IFilePathResolver resolver){
            return processIncludes(xElement, resolver, "");
        }

        public static XElement processIncludes(this XElement xElement, string defaultPrefix){
            return processIncludes(xElement, myapp.files, defaultPrefix);
        }

        public static XElement processIncludes(this XElement xElement, IFilePathResolver resolver, string defaultPrefix){
            return processIncludes(xElement, "", resolver, defaultPrefix);
        }

        public static XElement processIncludes(this XElement xElement, string includeelementname, IFilePathResolver resolver, string defaultPrefix){
            return processIncludes(xElement, includeelementname, null, resolver, defaultPrefix);
        }

        public static XElement processSelfIncludes(this XElement xElement, string includeelementname){
            return processIncludes(xElement, includeelementname, xElement, null, "");
        }

        public static XElement processIncludes(this XElement xElement, string includeelementname, XElement source){
            return processIncludes(xElement, includeelementname, source, null, "");
        }

        public static readonly IDictionary<string, bool> IncludeStatistics = new Dictionary<string, bool>();
        public static readonly IDictionary<string, bool> RecursiveStatistics = new Dictionary<string, bool>();
        public static bool CollectStatistics;
        public static XElement processIncludes(this XElement xElement, string includeelementname, XElement source, IFilePathResolver resolver, string defaultPrefix){
            if(includeelementname.noContent()){
                includeelementname = "_include";
            }
            XElement[] elements = null;
            int recursive = 0;
            while (true) {
                recursive++;
                elements = xElement.Descendants().Where(x=>x.Name.LocalName==includeelementname).ToArray();
                if (null == elements || 0 == elements.Length) break;
             
                foreach (var element in elements){
                    
                    var xpath = element.attr("xpath");
                    if(xpath.noContent()){
                        xpath = element.Value;
                    }
                    string sxpath = "";
                    if(CollectStatistics) {
                        sxpath = xpath.replace("@id='[^']*'", "@id='XXX'");
                    }
                    if (CollectStatistics) RecursiveStatistics[sxpath] = RecursiveStatistics.get(sxpath) || (recursive > 1);
                    
                    if (null!=source)
                    {
                        var embeds = source.XPathSelectElements(xpath).ToArray();
                        element.ReplaceWith(embeds);
                        if (CollectStatistics) {
                            
                            IncludeStatistics[sxpath] = IncludeStatistics.get(sxpath) || (null != embeds.FirstOrDefault());
                        }
                    }
                    else{
                        var file = defaultPrefix + "/" + element.attr("file");
                        file = file.Replace("//", "/");
                        var xtxt = resolver.Read(file);
                        if (xtxt.noContent()){
                            throw new Exception("cannot read xml file " + file + " or file is empty");
                        }
                        var xml = XElement.Parse(xtxt);

                        if (xpath.noContent()){
                            element.ReplaceWith(xml);
                        }
                        else{
                            element.ReplaceWith(xml.XPathSelectElements(xpath));
                        }
                    }
                }
            }
            return xElement;
        }
        


        public static bool hasAttribute(this XElement iter, string attrName)
        {
            return null != iter.Attribute(attrName);
        }

        public static T value<T>(this XElement iter, string attrName, T def)
        {
            var v = iter.attr(attrName);
            if (v.noContent()) return def;
            return v.to<T>();
        }



        public static IEnumerable<XElement> withAttribute(this IEnumerable<XElement> elements, string name, Func<XAttribute, bool> condition)
        {
            foreach (var element in elements)
            {
                XAttribute attribute = null;
                if (null != (attribute = element.Attribute(name)))
                {
                    bool proceed = true;
                    if (null != condition)
                    {
                        proceed = condition(attribute);
                    }
                    if (proceed)
                    {
                        yield return element;
                    }
                }
            }
        }


        [MigrationPropose]
        public static T open<T>(this T writer, string elementName) where T : XmlWriter
        {
            writer.WriteStartElement(elementName);
            return writer;
        }
        [MigrationPropose]
        public static T close<T>(this T writer) where T : XmlWriter
        {
            writer.WriteEndElement();
            return writer;
        }
[MigrationPropose]
        public static T map<T, TItem>(this T writer, IEnumerable<TItem> items, Action<T, TItem> serializer) where T : XmlWriter
        {
            foreach (var item in items)
            {
                serializer(writer, item);
            }
            return writer;
        }
        [MigrationPropose]
        public static T map<T, TItem>(this T writer, IEnumerable<TItem> items, string containerElementName) where T : XmlWriter
        {
            return writer.open(containerElementName).map(items, (w, i) => w.WriteElementString("item", i.ToString())).close();
        }


        [MigrationPropose]
        public static T write<T, I>(this T writer, string elementName, IEnumerable<I> range) where T : XmlWriter
        {
            foreach (var i in range)
            {
                writer.write(elementName, i);
            }
            return writer;
        }
        [MigrationPropose]
        public static T write<T, I>(this T writer, string elementName, I item) where T : XmlWriter
        {
            //TODO: DifferentLogic
            return writer.open(elementName).writeAttributes(item).close();
        }
        [MigrationPropose]
        public static T writeAttribute<T>(this T writer, string name, string value) where T : XmlWriter
        {
            writer.WriteAttributeString(name, String.Empty, value);
            return writer;
        }
        [MigrationPropose]
        public static T writeAttributes<T, I>(this T writer, I item) where T : XmlWriter
        {
            return (from i in
                        from prop in item.GetType().GetProperties()
                        where prop.PropertyType.IsValueType || prop.PropertyType.Equals(typeof(string))
                        select new { name = prop.Name.ToLower(), value = prop.GetValue(item, null) }
                    where i.value != null
                    select writer.writeAttribute(i.name, i.value.ToString())).Last();


        }




        public static T get<T>(this XElement element,string attribureOrElement){
            return get(element, attribureOrElement, default(T));
        }

        [MigrationPropose]
        public static T attr<T>(this T writer, string name, string value) where T : XmlWriter
        {
            writer.WriteAttributeString(name, value);
            return writer;
        }


        public static string attr(this XElement xml, string attrName){
            return attr(xml, attrName, "");
        }

        public static string findattr(this XElement xml, string attrName)
        {
            if(null==xml.Attributes().FirstOrDefault(x=>x.Name.LocalName==attrName)) {
                if(null!=xml.Parent) {
                    return findattr(xml.Parent, attrName);
                }
            }
            return xml.Attribute(attrName).Value;
        }

        public static T elementOrAttr<T>(this XElement xml, string  name, T def = default(T)) {
            if (xml == null) return def;
            var e = xml.Element(name);
            if (e != null) return e.Value.to<T>();
            return xml.attr(name, def);
        }
        public static T elementId<T>(this XElement xml, string name, T def = default(T))
        {
            if (xml == null) return def;
            var e = xml.Element(name);
            if (e == null) return def;
            return e.attr("id", def);
        }

        public static string attr(this XElement xml, string attrName, string def)
        {
            if (xml == null) return def;
            var a = xml.Attribute(attrName);
            if(a==null || a.Value.noContent()) return def;
            return a.Value;
        }

        public static string idorcode(this XElement xml, string def="") {
            var a = xml.Attribute("id");
            if(null!=a) return a.Value;
            a = xml.Attribute("code");
            if(null!=a) return a.Value;
            return def;
        }

        public static T attr<T>(this XElement xml, string attrName, T def = default(T))
        {
            if (xml == null) return def;
            var a = xml.Attribute(attrName);
            if (a == null || a.Value.noContent()) return def;
            return a.Value.to<T>();
        }

        public static XElement setattr(this XElement x,XName name, object value){
            x.SetAttributeValue(name,value);
            return x;
        }

        public static T serialize<T>(this XElement x) where T: new()
        {
            var result = new T();
            x.applyTo(result);
            return result;
        }
        public static IEnumerable<T> serialize<T>(this IEnumerable<XElement> x) where T : new()
        {
            foreach (var element in x) {
                yield return element.serialize<T>();
            }
        }

        public static string toStr(this XElement nav, string xpath)
        {
            var i = nav.XPathSelectElements(xpath);
            foreach (var element in i){
                return element.Value;
            }
            return "";
        }

        public static T get<T>(this XElement element, string attribureOrElement, T def)
        {
            if(null==element) return def;
            var attr = element.Attribute(attribureOrElement);
            if(null!=attr){
                if(attr.Value.noContent()) return def;
                return attr.Value.to<T>();
            }
            var el = element.Element(attribureOrElement);
            if(null!=el){
                if(el.Value.hasContent()){
                    return el.Value.to<T>();
                }
            }
            return def;
        }

        public static XElement First(this XElement x, string xpath)
        {
            return x.XPathSelectElements(xpath).FirstOrDefault();
        }

        public static IEnumerable<XElement> chooseFirstNotEmpty(this XElement nav, params  string[] variantXpaths)
        {
            IEnumerable<XElement> result = null;
            foreach (var xpath in variantXpaths)
            {
                result = nav.XPathSelectElements(xpath);
                if(result.Count()!=0) return result;
            }
            return new XElement[]{};
        }

        public static string getText(this XElement nav, string elementName){
            return getText(nav, elementName, "");
        }

        public static string getText(this XElement nav, string elementName,string def)
        {
            var e = nav.Element(elementName);
            if(null==e) return def;
            return e.Value;
        }

        public static string chooseAttr(this XElement xml, params string[] attrNames)
        {
            string result = "";
            foreach (var attrName in attrNames)
            {
                result = xml.attr(attrName, "");
                if (result.hasContent()) return result;
            }
            return result;
        }

        public static T applyTo<T>(this XElement navigator, T target)
        {
            if(null==navigator) {
                return target;
            }
            foreach (var attribute in navigator.Attributes())
            {
                target.setPropertySafe(attribute.Name.ToString(), attribute.Value);
            }
            if(navigator.Value.hasContent()){
                target.setPropertySafe("InitialStringValue", navigator.Value);
            }
            if(target is IWithXmlSource) {
                ((IWithXmlSource) target).Source = navigator;
            }
            return target;
        }

        public static IEnumerable<T> read<T>(this XElement nav, string xpath) where T : new()
        {
            var iter = nav.XPathSelectElements(xpath);
            foreach (var element in iter){
                var result = new T();
                element.applyTo(result);
                yield return result;
            }
            
        }

        public static XElement up(this XElement e, string  name) {
            var c = e.Parent;
            while (c!=null) {
                if (c.Name.LocalName == name) return c;
                c = c.Parent;
            }
            return c;
        }

        public static string item(this XElement nav, string ename)
        {
            return nav.XPathSelectElement("./" + ename).Value;
        }

        public static T item<T>(this XElement nav, string ename) where T : struct
        {
            return nav.item(ename).to<T>();
        }

        public static IEnumerable<T> items<T>(this XElement nav, string ename) where T : struct
        {
            return nav.XPathSelectElements("./" + ename).Select(n => n.Value.to<T>()
                );
        }

        public static XElement update(this XElement element, object target,params string[] excludes){
            var type = target.GetType();
            foreach (var attribute in element.Attributes()){
                if(excludes.Contains(attribute.Name.LocalName)) continue;
                var prop = type.resolveProperty(attribute.Name.LocalName);
                if(prop!=null){
                    target.setPropertySafe(prop.Name, attribute.Value);
                }
            }
            return element;
        }

        public static T updatefrom<T>(this T target, XElement x, params string[] excludes){
            x.update(target,excludes);
            return target;
        }

        public static T dataItem<T>(this XElement nav, string ename) where T : class
        {
            var code = nav.item(ename);
            return myapp.storage.Get<T>().First("Code = ?", code);
        }

        public static T freeDataItem<T>(this XElement nav, string ename) where T : class
        {
            var descriptor = nav.item(ename);
            if (descriptor.noContent()) return null;
            var query = new { Code = descriptor.Split(':')[0], Type = descriptor.Split(':')[1].toType() };
            return (T)myapp.storage.Get(query.Type).First(query.Type, "Code = '"+ query.Code + "'");
        }

        public static object dataItem(this XElement nav, Type targetType, string ename)
        {
            var code = nav.item(ename);
            return myapp.storage.Get(targetType).First(targetType, "Code ='"+ code+"'");
        }

        public static IXPathNavigable asXPathNavigable(this string xml)
        {
            if (xml.noContent())
            {
                return new XPathDocument(new StringReader("<xml />"));
            }
            return new XPathDocument(new StringReader(xml));
        }

        public static IEnumerable<string> readList(this XElement doc, string xpath)
        {
            var i = doc.XPathSelectElements(xpath);
            foreach (var x in i){
                
            
                yield return x.Value;
            }
        }
    }
}