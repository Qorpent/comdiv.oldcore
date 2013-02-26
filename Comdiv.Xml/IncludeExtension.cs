using System;
using System.IO;
using System.Linq;
using System.Xml;
using Comdiv.Extensions;

namespace Comdiv.Xml{
    public static class IncludeExtension{
        public const string includesNS = "urn://comdiv/xml/includes";

        public static void ResolveIncludes(this XmlDocument doc){
            if (null == doc) return;
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("i", includesNS);
            var includes = doc.SelectNodes("//i:include", ns).OfType<XmlElement>().ToArray();
            var uri = new Uri(doc.BaseURI);
            if (uri.Scheme != "file") throw new Exception("Only file:// schema of URI supported");
            var dpath = uri.PathAndQuery;
            var dir = Path.GetDirectoryName(dpath);
            foreach (var include in includes){
                var parent = include.ParentNode;

                var path = include.GetAttribute("path");
                if (path.noContent()) path = doc.BaseURI;
                if (!Path.IsPathRooted(path)) path = Path.Combine(dir, path);
                if (new FileInfo(path).FullName == new FileInfo(dpath).FullName)
                    throw new Exception("Recursive includes are not allowed");
                var xpath = include.GetAttribute("xpath");
                if (xpath.noContent()) xpath = "/*";

                var includeDoc = new XmlDocument();
                includeDoc.Load(IncludeAwareXmlReader.Create(path));
                var inputNodes = includeDoc.SelectNodes(xpath).OfType<XmlNode>().ToArray();

                foreach (var node in inputNodes){
                    var imported = doc.ImportNode(node, true);
                    parent.InsertBefore(imported, include);
                }

                parent.RemoveChild(include);
            }
        }
    }
}