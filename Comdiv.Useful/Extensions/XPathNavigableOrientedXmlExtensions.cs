using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.IO;
using Comdiv.Model;
using Comdiv.Persistence;

namespace Comdiv.Extensions{
    public static class XPathNavigableOrientedXmlExtensions{
        public static IXPathNavigable asXmlFile(this string path)
        {
            return new XPathDocument(path);
        }
        //public static T getProperty<T>(this object target, string name)
        //{
        //    return getProperty(target, name).to<T>();
        //}

        

        public static string item(this XPathNavigator nav, string ename)
        {
            return nav.SelectSingleNode("./" + ename).Value;
        }

        public static T item<T>(this XPathNavigator nav, string ename) where T : struct
        {
            return nav.item(ename).to<T>();
        }

        public static IEnumerable<T> items<T>(this XPathNavigator nav, string ename) where T : struct
        {
            return nav.Select("./" + ename).Select(n => n.Value.to<T>()
                );
        }

        public static T dataItem<T>(this XPathNavigator nav, string ename) where T : class
        {
            var code = nav.item(ename);
            return myapp.storage.Get<T>().First("Code", code);
        }

        public static T freeDataItem<T>(this XPathNavigator nav, string ename) where T : class
        {
            var descriptor = nav.item(ename);
            if (descriptor.noContent()) return null;
            var query = new { Code = descriptor.Split(':')[0], Type = descriptor.Split(':')[1].toType() };
            return myapp.storage.Get(query.Type).Query(query.Type, "Code", query.Code).Cast<T>().FirstOrDefault();
        }

        public static object dataItem(this XPathNavigator nav, Type targetType, string ename)
        {
            var code = nav.item(ename);
            return myapp.storage.Get(targetType).Query(targetType, "Code", code).Cast<object>().FirstOrDefault();
        }



        public static bool hasAttribute(this XPathNodeIterator iter, string attrName)
        {
            return (iter.Current.GetAttribute(attrName, "").hasContent());
        }
        
        public static XPathNodeIterator Select(this IXPathNavigable x, string xpath)
        {
            return x.CreateNavigator().Select(xpath);
        }

        public static IEnumerable<string> readList(this IXPathNavigable doc, string xpath)
        {
            var nav = doc.CreateNavigator();
            var i = nav.Select(xpath);
            while (i.MoveNext())
            {
                yield return i.Current.Value;
            }
        }


        public static T value<T>(this XPathNodeIterator iter, string attrName, T def)
        {
            var v = iter.attr(attrName);
            if (v.noContent()) return def;
            return (T)Convert.ChangeType(v, typeof(T));
        }

        public static string value(this XPathNodeIterator iter)
        {
            return iter.Current.Value;
        }


        public static XPathNavigator First(this IXPathNavigable x, string xpath)
        {
            var i = x.Select(xpath);
            if (i.MoveNext()) return i.Current;
            return null;
        }

        public static IEnumerable<T> Select<T>(this XPathNodeIterator iter, Func<XPathNavigator, T> converter)
        {
            while (iter.MoveNext()) yield return converter(iter.Current);
        }

        //public static IEnumerable<T> Select<T>(this IEn iter)
        //    where T : Comdiv.Xml.IXmlReadable, new()
        //{
        //    while (iter.MoveNext())
        //    {
        //        var result = new T();
        //        result.ReadFromXml(iter.Current);
        //        yield return result;
        //    }
        //}

        public static string getRootAttribute(this IXPathNavigable x, string name)
        {
            if (null == x) return null;
            var n = x.CreateNavigator();
            var root = n.SelectSingleNode("/*");
            if (!root.MoveToAttribute(name, String.Empty)) return null;
            return root.Value;
        }

        public static XPathNodeIterator chooseFirstNotEmpty(this XPathNavigator nav, params  string[] variantXpaths)
        {
            XPathNodeIterator iter = null;
            foreach (var xpath in variantXpaths)
            {
                iter = nav.Select(xpath);
                if (iter.Count != 0) return iter;
            }
            return iter;
        }

        public static string getText(this XPathNavigator nav, string elementName)
        {
            return nav.Evaluate("string(./" + elementName + ")").ToString().Trim();
        }

        public static IEnumerable<Entity> getAttributes(this XPathNavigator navigator)
        {
            var nav = navigator.Clone();
            if (nav.MoveToFirstAttribute())
            {
                do
                {
                    yield return new Entity { Code = nav.Name, Name = nav.Value };
                } while (nav.MoveToNextAttribute());
            }
        }
        public static object getProperty(this object target, string name)
        {
            if (null == target)
                throw new InvalidOperationException(String.Format("cannot retrieve property {0} of NULL", name));
            var prop = target.GetType().GetProperty(name);
            if (null == prop)
                throw new InvalidOperationException(String.Format("No property {0} on class {1}", name, target.GetType()));
            return target.GetType().GetProperty(name).GetValue(target, null);
        }
        public static string chooseAttr(this XPathNavigator nav, params string[] attrNames)
        {
            string result = "";
            foreach (var attrName in attrNames)
            {
                result = nav.GetAttribute(attrName, "");
                if (result.hasContent()) return result;
            }
            return result;
        }

        public static T applyTo<T>(this XPathNavigator navigator, T target)
        {
            foreach (var attribute in navigator.getAttributes())
            {
                target.setPropertySafe(attribute.Code, attribute.Name);
            }
            return target;
        }


        public static IEnumerable<T> read<T>(this XPathNavigator nav, string xpath) where T : new()
        {
            var iter = nav.Select(xpath);
            while (iter.MoveNext())
            {
                var result = new T();
                iter.Current.applyTo(result);
                yield return result;
            }
        }

        [MigrationPropose]
        public static string toStr(this IXPathNavigable nav, string xpath)
        {
            return nav.CreateNavigator().toStr(xpath);
        }

        [MigrationPropose]
        public static string toStr(this XPathNavigator nav, string xpath)
        {
            var i = nav.Select(xpath);
            while (i.MoveNext())
            {
                return i.Current.Value;
            }
            return String.Empty;
        }

        [MigrationPropose]
        public static string attr(this XPathNodeIterator iter, string attrName)
        {
            return iter.Current.GetAttribute(attrName, String.Empty);
        }

        [MigrationPropose]
        public static string attr(this XPathNavigator nav, string attrName)
        {
            return nav.GetAttribute(attrName, String.Empty);
        }

        [MigrationPropose]
        public static string attr(this XPathNodeIterator iter, string attrName, string defaultValue)
        {
            var result = iter.attr(attrName);
            if (result.noContent()) return defaultValue;
            return result;
        }
    }
}