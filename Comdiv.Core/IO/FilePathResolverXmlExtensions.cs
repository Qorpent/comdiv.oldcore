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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Comdiv.Design;
using Comdiv.Extensions;

namespace Comdiv.IO{
    ///<summary>
    ///</summary>
    public static class FilePathResolverXmlExtensions{
        [NoCover("cannot cover both mono and .net code")]
        static FilePathResolverXmlExtensions(){
            if (typeof (string).Assembly.GetCustomAttributes(typeof (AssemblyCopyrightAttribute), false).
                    FirstOrDefault(a => ((AssemblyCopyrightAttribute) a).Copyright.Contains("Micros")) != null){
                XmlMergeXslt = Assembly.GetExecutingAssembly().readResource("Comdiv.IO.x-merge.xslt");
                ConfigMergeXslt = Assembly.GetExecutingAssembly().readResource("Comdiv.IO.cs-merge.xslt");
            }
            else{
                XmlMergeXslt = Assembly.GetExecutingAssembly().readResource("Comdiv.IO.x-merge-mono.xslt");
                ConfigMergeXslt = Assembly.GetExecutingAssembly().readResource("Comdiv.IO.cs-merge.xslt");
            }
        }

        ///<summary>
        /// xslt for configuration merge
        ///</summary>
        public static string ConfigMergeXslt { get; set; }

        /// <summary>
        /// Gets the XML merge XSLT.
        /// </summary>
        /// <value>The XML merge XSLT.</value>
        public static string XmlMergeXslt { get; set; }

        /// <summary>
        /// Reads the XML with merging and XSLT support.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string ReadXml(this IFilePathResolver resolver, string path){
            var parsedPath = Regex.Match(path,@"^(\+)?([^\?\+]+)(\?([\s\S]*))?$");
            if(!parsedPath.Success){
                throw new ArgumentException("invalid format for readxml " + path, "path");
            }
            path = parsedPath.Groups[2].Value;
            var xpath = parsedPath.Groups[4].Value;
            var merge = parsedPath.Groups[1].Value.hasContent();
            return resolver.ReadXml(path, xpath, new ReadXmlOptions() { Merge = merge});
        }

        private static string getValue(string result, string xpath){
            var res = new XPathDocument(new StringReader(result)).CreateNavigator().Evaluate(xpath);
            if (res is XPathNodeIterator){
                if (((XPathNodeIterator) res).MoveNext()){
                    res = ((XPathNodeIterator) res).Current.Value;
                }
                else{
                    return String.Empty;
                }
            }
            return null == res ? String.Empty : res.ToString();
        }

        private static string getNodes(string xpath, string result){
            var sw = new StringWriter();
            var sets = new XmlWriterSettings{OmitXmlDeclaration = true};
            var xw = XmlWriter.Create(sw, sets);

            xw.WriteStartElement("__");
            var iter = new XPathDocument(new StringReader(result)).CreateNavigator().Select(xpath);
            while (iter.MoveNext()){
                if (iter.Current.NodeType == XPathNodeType.Attribute)
                {
                    xw.WriteElementString("_"+iter.Current.Name,iter.Current.Value);
                }
                else{
                    iter.Current.WriteSubtree(xw);
                }
            }
            xw.WriteEndElement();
            xw.Flush();
            result = sw.ToString();
            return result;
        }

        /// <summary>
        /// Gets the query result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        public static string GetQueryResult(this string result, string xpath){
            if (xpath.StartsWith("$")){
                xpath = "!//option[@key='" + xpath.Substring(1) + "']/text()";
            }
            if (!String.IsNullOrEmpty(xpath)){
                if (xpath.StartsWith("!")){
                    return getValue(result, xpath.Substring(1));
                }
                result = getNodes(xpath, result);
            }
            return result;
        }

        /// <summary>
        /// Performs xslt transformation.
        /// </summary>
        /// <param name="system">The system.</param>
        /// <param name="transformation">The transformation.</param>
        /// <param name="result">The result.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static string XsltTransform(this IFilePathResolver system, string transformation, string result,
                                           XsltArgumentList args){
            var xslt = system.Read(transformation);
            var resultWriter = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.OmitXmlDeclaration = true;
            var xwriter = XmlWriter.Create(resultWriter,settings);
            system.DefinePseudoFile(".this",result);
            var resolver = new FilePathXmlResolver(system);
            system.DefinePseudoFile(".my", xslt);
            var tr = new XslCompiledTransform();
            tr.Load(new XPathDocument(new StringReader(xslt)), XsltSettings.TrustedXslt, resolver);

            var reader = XmlReader.Create(new StringReader(result));

            tr.Transform(reader, args, xwriter, resolver);
            xwriter.Flush();
            result = resultWriter.ToString();

            return result;
        }

        /// <summary>
        /// Gets the transformation result.
        /// </summary>
        /// <param name="system">The system.</param>
        /// <param name="transformations">The transformations.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static string GetTransformationResult(this IFilePathResolver system, IEnumerable<string> transformations,
                                                     string result){
            foreach (var transformation in transformations){
                result = system.XsltTransform(transformation, result, null);
            }
            return result;
        }

        /// <summary>
        /// Merges the XML.
        /// </summary>
        /// <param name="system">The system.</param>
        /// <param name="xml1">The XML1.</param>
        /// <param name="xml2">The XML2.</param>
        /// <returns></returns>
        public static string MergeXml(this IFilePathResolver system, string xml1, string xml2){
            if(xml1.noContent()||xml2.noContent()) return xml1.noContent() ? xml2 : xml1;
            var args = new XsltArgumentList();
            var src = new XPathDocument(new StringReader(xml2)).CreateNavigator();
            src.MoveToRoot();
            args.AddParam("src", String.Empty, src);
            var result = system.XsltTransform(".xmerge.xslt", xml1, args);
            result = Regex.Replace(result, @"</?m:\w+[^\>]*?>", String.Empty, RegexOptions.Compiled);
            return result;
        }

        /// <summary>
        /// Reads the XML (can use cache).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static string ReadXml(this IFilePathResolver resolver, string path, string xpath){
            return ReadXml(resolver, path, xpath, new ReadXmlOptions());
        }

        /// <summary>
        /// Reads the XML (can use cache).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static string ReadXml(this IFilePathResolver resolver, string path, string xpath, ReadXmlOptions options){
            checkResolverIsPreparedForXmlProcessing(resolver);
            var result = String.Empty;
            var basePath = resolver.Resolve(path);
            if (null != basePath){
                result = options.Merge ? resolver.MergeXml(path) : resolver.Read(path);

                if(options.UseIncludes){
                    var x = XElement.Parse(result);
                    x = x.processIncludes(resolver, options.IncludeRoot);
                    result = x.ToString();
                }


                if (options.ApplyXslt && !options.XsltAfterXpath){
                    result = resolver.GetTransformationResult(resolver.resolveTransformators(path), result);
                }

                if (!String.IsNullOrEmpty(xpath)){
                    result = result.GetQueryResult(xpath);
                }

                if (options.ApplyXslt && options.XsltAfterXpath)
                {
                    result = resolver.GetTransformationResult(resolver.resolveTransformators(path), result);
                }

            }
            if (String.IsNullOrEmpty(result)){
                result = String.Empty;
            }

            return result;
        }

        private static void checkResolverIsPreparedForXmlProcessing(IFilePathResolver resolver){
            if (!resolver.Data.ContainsKey("__xml__resolver_prepared")){
                resolver.DefinePseudoFile(".default.xml", "<__ />");
                // the usefull default xslt for import - can work with cce:option element and config mode=append
                resolver.DefinePseudoFile(".default.xslt", ConfigMergeXslt);
                resolver.DefinePseudoFile(".xmerge.xslt", XmlMergeXslt);
                resolver.Data["__xml__resolver_prepared"] = true;
            }
        }

        private static IEnumerable<string> resolveTransformators(this IFilePathResolver resolver, string configName){
            var append = configName + ".xslt";
            if (append.Contains(".xml")){
                append = append.Replace(".xml", String.Empty);
            }
            return resolver.ResolveAll(append);
        }

        private static string MergeXml(this IFilePathResolver resolver, string path){
            var result = String.Empty;
            foreach (var c in resolver.ResolveAll(path)){
                result = resolver.MergeXml(result, File.ReadAllText(c));
            }
            return result;
        }
    }
}