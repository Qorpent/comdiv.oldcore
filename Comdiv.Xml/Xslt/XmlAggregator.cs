#region

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Comdiv.Xml;

#endregion

namespace Comdiv.Xslt{

    #region

    #endregion

    /// <summary>
    /// Summary description for XmlAggregator.
    /// </summary>
   [Obsolete]
    public class XmlAggregator{
        public static XPathNodeIterator Distinct(XPathNodeIterator source, string by, string ns){
            return Distinct(source, by, ns, null);
        }

        public static XPathNodeIterator Distinct(XPathNodeIterator source, string by, string ns, object ext){
            var res = new StringBuilder();
            var sw = new StringWriter(res);
            var w = new XmlTextWriter(sw);
            w.WriteStartElement("root");
            var uniques = new Hashtable();
            XmlNamespaceManager m = null;
            XPathExpression exp = null;
            var compiled = false;
            while (source.MoveNext()){
                if (!compiled){
                    m = new XsltSimpleContext(ext, (NameTable) source.Current.NameTable);
                    //m.PushScope();
                    //m.AddNamespace(source.Current.Prefix,source.Current.NamespaceURI);
                    //	m = new XmlNamespaceManager(source.Current.NameTable);
                    m.PushScope();
                    if (ns.Length != 0){
                        foreach (var nsdecl in ns.Split(';')){
                            var ns_ = nsdecl.Split('=');
                            m.AddNamespace(ns_[0], ns_[1]);
                        }
                    }


                    exp = source.Current.Compile(by);
                    exp.SetContext(m);
                    compiled = true;
                }
                object key = null;
                try{
                    key = source.Current.Evaluate(exp);
                }
                catch (Exception ex){
                    throw ex;
                }
                if (!uniques.ContainsKey(key))
                    uniques.Add(key, source.Current.Clone());
            }
            foreach (var val in uniques.Values)
                XmlUtil.WriteNavigator(w, (XPathNavigator) val, true);
            w.WriteEndElement();
            w.Flush();
            return XmlUtil.GetIterator(res.ToString());
        }

        public static XPathNodeIterator Distinct(XPathNodeIterator source, string by){
            return Distinct(source, by, string.Empty);
        }
    }
}