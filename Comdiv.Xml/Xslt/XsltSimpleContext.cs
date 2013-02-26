#region

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using Comdiv.Transformation;

#endregion

namespace Comdiv.Xslt{

    #region

    #endregion

    /// <summary>
    /// Summary description for XsltSimpleContext.
    /// </summary>
    [Obsolete]
    public class XsltSimpleContext : XsltContext{
        private readonly Hashtable extensions = new Hashtable();
        private readonly Hashtable h = new Hashtable();
        private readonly ITransformator transformator;

        public XsltSimpleContext(object extension, NameTable t) : base(t){
            Extensions[string.Empty] = extension;
        }

        public XsltSimpleContext(object extension){
            Extensions[string.Empty] = extension;
        }

        public XsltSimpleContext(ITransformator trans, object ext)
            : base(new NameTable()){
            transformator = trans;
            Extensions[""] = ext;
        }


        public override bool Whitespace{
            get { return false; }
        }

        public Hashtable Extensions{
            get { return extensions; }
        }

        public override IXsltContextVariable ResolveVariable(string prefix, string name){
             if (transformator != null) return new XsltSimpleContextVariable(name, transformator);
            var sel = prefix;
            if (Extensions[sel] == null) sel = string.Empty;
            return new XsltSimpleContextVariable(Extensions[sel], name);
        }

        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes){
            var sel = prefix;
            if (Extensions[sel] == null) sel = string.Empty;
            return new XsltSimpleContextFunction(Extensions[sel], name);
        }

        public override int CompareDocument(string baseUri, string nextbaseUri){
            return 0;
        }

        public override bool PreserveWhitespace(XPathNavigator node){
            return false;
        }

        public override void AddNamespace(string prefix, string uri){
            base.AddNamespace(prefix, uri);
            h[prefix] = uri;
        }

        public override void RemoveNamespace(string prefix, string uri){
            base.RemoveNamespace(prefix, uri);
            h.Remove(prefix);
        }

        public override string LookupNamespace(string prefix){
            var res = base.LookupNamespace(prefix);
            if (res == null)
                return h[prefix].ToString();
            return res;
        }
    }
}