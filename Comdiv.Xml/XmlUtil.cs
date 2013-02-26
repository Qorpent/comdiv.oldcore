using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Comdiv.Extensions;

namespace Comdiv.Xml{
    [Obsolete]
    /// <summary>
        /// Утилитный класс для быстрых операций над XML
        /// </summary>
    public class XmlUtil{
        private const string mtxx_ns = "MT-xml-extensions";
        private const string mtxx_prefix = "mtxx";

        private const string NsChanger =
            @"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:msxsl='urn:schemas-microsoft-com:xslt' xmlns:this='this#script' exclude-result-prefixes='this msxsl sys' xmlns:sys='urn:ariis:xslt:system' xmlns:{2}='{1}'>
	<xsl:output method='xml' version='1.0' encoding='UTF-8' indent='yes'/>
	<xsl:template match='node()|@*'>
		<xsl:copy>
			<xsl:apply-templates select='node()|@*'/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match='*[namespace-uri(.)=""{0}""]'>
		<xsl:element name='{2}:{{local-name(.)}}' namespace='{1}'>
			<xsl:apply-templates select='node()|@*'/>
		</xsl:element>
	</xsl:template>
	<xsl:template match='@*[namespace-uri(.)=""{0}"" and namespace-uri(.)!=""""]'>
		<xsl:attribute name='{2}:{{local-name(.)}}' namespace='{1}'><xsl:value-of select='.'/></xsl:attribute>
	</xsl:template>
</xsl:stylesheet>
";

        /*
         * 
         * 			Матрица преобразований
         * 
         * 				Строка			SAX			DOM			Doc			Nav		Url
         * Строка		-----		ToReader*		ToDom!		ToDoc		ToNav	Save
         * SAX			ToString!	---------		ToDom!		ToDoc!		ToNav!	Save
         * DOM			+++++		+++++++			------		ToDoc		++++++	++++
         * Doc			ToString!	ToReader!		ToDom!		-----		++++++	Save
         * Nav			ToString!	ToReader!		ToDom!		ToDoc		------	Save	
         * Url			Load		Load			++++++		+++++		Load	Copy
         * XPath/Eval	Evaluate	Evaluate!		Evaluate!	Evaluate!	++++++	Evaluate!
         * XPath/Select	Select		Select!			Select!		Select!		++++++	Select!
         * XSLT			Transform	Transform		Transform	Transform	Transform	Transform
         * 
         * */

        private static XslCompiledTransform ChNsTr;
        private static string cmplNsChanger = "";

        #region STRING ConvertTo XML FORMATS

        /// <summary>
        /// Конвертирует строка - XmlReader
        /// </summary>
        /// <param name="data">строка, содержащая XML</param>
        /// <returns></returns>
        public static XmlReader GetReader(string data){
            return XmlReader.Create(new StringReader(data));
        }

        #endregion

        /// <summary>
        /// Добавляет текст <b>include</b> в конец главного элемента документа
        /// <b>targetxml</b>
        /// </summary>
        /// <param name="include">Текст для вставки в XML</param>
        /// <param name="targetxml">Целевой документ</param>
        /// <returns></returns>
        public static string IncludeXml(string include, string targetxml){
            var sw = new StringWriter();
            using (var w = XmlWriter.Create(sw)){
                var inc = RemoveDeclaration(include);
                using (var r = XmlReader.Create(new StringReader(targetxml))){
                    while (r.Read()){
                        if (r.NodeType == XmlNodeType.Element){
                            w.WriteStartElement(r.Prefix, r.LocalName, r.NamespaceURI);
                            if (r.MoveToFirstAttribute()){
                                w.WriteNode(r, false);
                                while (r.MoveToNextAttribute()) w.WriteNode(r, false);
                                r.MoveToElement();
                            }
                            var inner = r.ReadInnerXml();
                            w.WriteRaw(inner);
                            w.WriteRaw(inc);
                            w.WriteEndElement();
                            break;
                        }
                    }
                }
            }
            return sw.ToString();
        }

        private static XslCompiledTransform getChNStransform(string fromns, string tons, string prefix){
            var st = string.Format(NsChanger, fromns, tons, prefix);
            if (cmplNsChanger != st){
                cmplNsChanger = st;
                ChNsTr = new XslCompiledTransform();

                ChNsTr.Load(GetDoc(st), new XsltSettings(false, false), new XmlUrlResolver());
            }
            return ChNsTr;
        }

        public static string RemoveDeclaration(string xml){
            if (xml.IndexOf("<?xml") == -1) return xml;
            return Regex.Replace(xml, "<\\?xml[^?]+\\?>", "", RegexOptions.Compiled);
        }

        public static string ChangeNamespace(string data, string fromns, string tons, string prefix){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            getChNStransform(fromns, tons, prefix).Transform(XmlReader.Create(new StringReader(data)), null, w,
                                                             new XmlUrlResolver());
            return sw.ToString();
        }

        public static string Escape(string data){
            var d = new XmlDocument();
            var root = d.CreateElement("root");
            root.InnerText = data;
            return root.InnerXml;
        }

        public static string UnEscape(string data){
            var d = new XmlDocument();
            var root = d.CreateElement("root");
            root.InnerXml = data;
            return root.InnerText;
        }

        public static string ReadFile(string path, bool skipdeclaration){
            var res = new XmlUrlResolver();
            var uri = res.ResolveUri(null, path);
            var s = (Stream) res.GetEntity(uri, "standard", typeof (Stream));
            try{
                var sw = new StringWriter();
                var w = XmlWriter.Create(sw);
                var r = XmlReader.Create(s);
                if (skipdeclaration){
                    r.Read();
                    if (!(r.NodeType == XmlNodeType.XmlDeclaration)) w.WriteNode(r, false);
                    else r.Read();
                }
                while (!r.EOF) w.WriteNode(r, false);
                w.Flush();
                return sw.ToString();
            }
            finally{
                s.Close();
            }
        }

        public static string ReadFile(string path){
            return ReadFile(path, true);
        }

        public static void WriteFile(string data, string path){
            XmlWriter w = new XmlTextWriter(path, Encoding.UTF8);
            try{
                w.WriteStartDocument(true);
                w.WriteRaw(data);
            }
            finally{
                w.Close();
            }
        }

        public static XPathDocument GetDoc(string data){
            if (data.noContent()) return null;
            return new XPathDocument(new StringReader(data.Replace((char) 26, (char) 32)));
        }

        public static XPathDocument GetDoc(Stream stream){
            return new XPathDocument(stream);
        }

        public static XPathNavigator GetNavigator(string data){
            return GetDoc(data).CreateNavigator();
        }

        public static XPathNodeIterator GetIterator(string data){
            return GetNavigator(data).Select("/");
        }

        public static string WrapAsXml(string data){
            var w = new StringWriter();
            w.Write("<root>");
            w.Write(data);
            w.Write("</root>");
            return w.ToString();
        }

        public static string WrapAsText(string data, string elementName, string ns){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            w.WriteElementString(elementName, ns, data);
            return sw.ToString();
        }

        public static string WrapAsText(string data){
            return WrapAsText(data, "root", "");
        }

        public static string WrapAsTextIfNeeded(string data, XmlTestDepth depth){
            if (!XmlTestBase.GetIsXml(data, depth)) return WrapAsText(data);
            return data;
        }


        public static void Write(XmlWriter w, XPathNavigator i){
            WriteNavigator(w, i, true);
        }

        public static void Write(XmlWriter w, XPathNodeIterator i){
            while (i.MoveNext()) WriteNavigator(w, i.Current, false);
        }

        public static void Write(XmlWriter w, string data, string xpath){
            Write(w, Select(data, xpath));
        }

        public static string Write(XPathNavigator i){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            WriteNavigator(w, i, true);
            return sw.ToString();
        }

        public static string Write(XPathNodeIterator i){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            while (i.MoveNext()) WriteNavigator(w, i.Current, false);
            return sw.ToString();
        }

        public static string Write(string data, string xpath){
            return Write(Select(data, xpath));
        }

        public static void WriteNavigator(XmlWriter w, XPathNavigator i, bool start){
            var current = i.Clone();
            if (current.NodeType == XPathNodeType.Root) current.MoveToFirstChild();

            if (current.NodeType != XPathNodeType.Root){
                w.WriteStartElement(current.Prefix, current.NodeType == XPathNodeType.Root ? "root" : current.LocalName,
                                    current.NamespaceURI);
            }

            if (start){
                var ns = current.Clone();
                var res = ns.MoveToFirstNamespace();
                var checkedNs = new List<string>();
                while (res || ns.MoveToNextNamespace()){
                    res = false;
                    if (ns.Name != current.Prefix && ns.Name != "xml" && ns.Name != "xmlns" && ns.Name.Length != 0){
                        if (current.GetAttribute(ns.Name, "http://www.w3.org/XML/1998/namespace").noContent()){
                            if (!checkedNs.Contains(ns.Name)){
                                w.WriteAttributeString("xmlns:" + ns.Name, ns.Value);
                                checkedNs.Add(ns.Name);
                            }
                        }
                    }
                    if (ns.Name.Length == 0)
                        w.WriteAttributeString("xmlns", ns.Value);
                }
            }
            if (current.HasAttributes){
                var a = current.Clone();
                a.MoveToFirstAttribute();
                w.WriteAttributeString(a.Prefix, a.LocalName, a.NamespaceURI, a.Value);
                while (a.MoveToNextAttribute())
                    w.WriteAttributeString(a.Prefix, a.LocalName, a.NamespaceURI, a.Value);
            }
            if (current.HasChildren){
                var ch = current.Clone();
                ch.MoveToFirstChild();
                if (ch.NodeType == XPathNodeType.Element)
                    WriteNavigator(w, ch, false);
                else
                    w.WriteString(ch.Value);

                while (ch.MoveToNext()){
                    if (ch.NodeType == XPathNodeType.Element)
                        WriteNavigator(w, ch, false);
                    else
                        w.WriteString(ch.Value);
                }
            }
            w.WriteEndElement();
        }

        public static string Join(XPathNodeIterator i, string joiner){
            var b = new StringBuilder();
            while (i.MoveNext()){
                if (b.Length != 0) b.Append(joiner);
                b.Append(i.Current.Value);
            }
            return b.ToString().Trim();
        }

        public static string NodeSetToString(XPathNodeIterator i){
            var b = new StringBuilder();
            var sw = new StringWriter(b);
            var w = new XmlTextWriter(sw);
            while (i.MoveNext())
                WriteNavigator(w, i.Current, true);
            w.Flush();
            w.Close();
            return b.ToString();
        }

        public static string XPathFormatString(string pattern, XPathNodeIterator iter){
            var list = new ArrayList();
            while (iter.MoveNext()) list.Add(iter.Current.Value);
            return string.Format(pattern, list.ToArray());
        }

        public static XPathNodeIterator NodeSetFromString(string data){
            var xml = data;
            if (!data.StartsWith("<?xml"))
                xml = string.Format("<?xml version='1.0' ?><root>{0}</root>", data);
            return GetIterator(xml);
        }

        public static XPathDocument LoadDoc(string name, XmlResolver resolver){
            var uri = resolver.ResolveUri(null, name);
            var s = resolver.GetEntity(uri, "", typeof (Stream)) as Stream;
            return GetDoc(s);
        }

        public static XPathDocument LoadDoc(Uri uri, XmlResolver resolver){
            var s = resolver.GetEntity(uri, "", typeof (Stream)) as Stream;
            return GetDoc(s);
        }

        public static string NavigatorToString(XPathNavigator nav){
            var sw = new StringWriter();
            var w = new XmlTextWriter(sw);
            //nav.WriteSubtree(w);
            WriteNavigator(w, nav, true);
            return sw.ToString();
        }

        public static Stream NavigatorToStream(XPathNavigator nav){
            var sw = new MemoryStream();
            var w = new XmlTextWriter(sw, Encoding.UTF8);
            nav.WriteSubtree(w);
            w.Flush();
            sw.Position = 0;
            return sw;
        }

        public static XPathDocument NavigatorToDoc(XPathNavigator nav){
            return GetDoc(NavigatorToStream(nav));
        }

        public static object Evaluate(string data, string xpath){
            return Evaluate(data, xpath, null);
        }

        public static object Evaluate(string data, string xpath, XsltContext context){
            var data_ = data;
            if (data == null || data.Length == 0) data_ = "<root/>";
            if (!data.StartsWith("<?xml")) data_ = WrapAsXml(data);
            var nav = GetNavigator(data_);
            var ex = nav.Compile(xpath);
            if (context != null) ex.SetContext(context);
            return nav.Evaluate(ex);
        }

        public static object EvaluateFile(string path, string xpath, XsltContext context){
            return Evaluate(ReadFile(path), xpath, context);
        }

        public static string MergeXmlFiles(string basedir, string pattern, bool recursive){
            var sw = new StringWriter();
            var w = new XmlTextWriter(sw);
            //w.WriteStartDocument(true);
            w.WriteStartElement(mtxx_prefix, "file-merge", mtxx_ns);
            writeXmlFiles(w, basedir, pattern, recursive);
            w.WriteEndElement();
            var res = sw.ToString();
            res = Regex.Replace(res, @"<\?xml\s[^>]+\?>", "");
            return res;
        }

        public static void UpdateFilesFromMergedVersion(string fn){
            var r = XmlReader.Create(fn);
            while (r.Read()){
                if (r.NodeType == XmlNodeType.Element && r.NamespaceURI == mtxx_ns && r.LocalName == "file"){
                    var fname = r.GetAttribute("path");
                    r.Read();
                    while (r.NodeType != XmlNodeType.Element && r.NodeType != XmlNodeType.ProcessingInstruction)
                        r.Read();
                    if (r.NamespaceURI != mtxx_ns){
                        var data = r.ReadOuterXml();
                        var sw = new StreamWriter(fname, false, Encoding.UTF8);
                        try{
                            sw.Write("<?xml version='1.0' encoding='utf-8' ?>");
                            sw.Write(data);
                            sw.Flush();
                        }
                        finally{
                            sw.Close();
                        }
                    }
                    else{
                        if (r.LocalName == "io_error") continue;
                        if (r.LocalName == "delete"){
                            File.Delete(fname);
                            continue;
                        }
                        if (r.LocalName == "move"){
                            var to = r.GetAttribute("path");
                            File.Move(fname, to);
                            continue;
                        }
                        if (r.LocalName == "copy"){
                            var to = r.GetAttribute("path");
                            File.Copy(fname, to);
                            continue;
                        }
                    }
                }
            }
        }

        protected static void writeXmlFiles(XmlWriter w, string basedir, string pattern, bool recursive){
            w.WriteStartElement(mtxx_prefix, "directory", mtxx_ns);
            w.WriteAttributeString("path", basedir);
            if (recursive){
                foreach (var dn in Directory.GetDirectories(basedir))
                    writeXmlFiles(w, dn, pattern, recursive);
            }
            foreach (var fn in Directory.GetFiles(basedir, pattern))
                writeXmlFile(w, fn);

            w.WriteEndElement();
        }

        protected static void writeXmlFile(XmlWriter w, string fn){
            w.WriteStartElement(mtxx_prefix, "file", mtxx_ns);
            w.WriteAttributeString("path", fn);
            XmlReader r = null;
            try{
                r = XmlReader.Create(fn);
                w.WriteNode(r, false);
            }
            catch (IOException ex){
                w.WriteStartElement(mtxx_prefix, "io_error", mtxx_ns);
                w.WriteString(ex.Message);
                w.WriteEndElement();
            }
            finally{
                r.Close();
            }

            w.WriteEndElement();
        }

        public static string GetSimpleXmlElement(string name, string[] attribnames, string[] attribvalues){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            w.WriteStartElement(name);
            for (var i = 0; i < attribnames.Length; i++)
                w.WriteAttributeString(attribnames[i], attribvalues[i]);
            w.WriteEndElement();
            return sw.ToString();
        }

        public static string GetSimpleXmlElement(string name, string prefix, string[] attribnames, string[] attribvalues){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            w.WriteStartElement(prefix, name, "");
            for (var i = 0; i < attribnames.Length; i++)
                w.WriteAttributeString(attribnames[i], attribvalues[i]);
            w.WriteEndElement();
            return sw.ToString();
        }

        public static XPathNodeIterator Select(string doc, string xpath){
            return Select(doc, xpath, null);
        }

        public static XPathNodeIterator Select(string doc, string xpath, XmlNamespaceManager m){
            var nav = GetNavigator(doc);
            var exp = nav.Compile(xpath);
            if (m != null) exp.SetContext(m);
            return nav.Select(exp);
        }
    }
}