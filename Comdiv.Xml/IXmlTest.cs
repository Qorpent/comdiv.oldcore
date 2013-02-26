using System;
using System.Collections;
using System.IO;
using System.Xml.XPath;
using System.Xml.Xsl;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Xslt;


namespace Comdiv.Xml{
    /// <summary>
    /// Summary description for IXmlTest.
    /// </summary>
    /// 
    [Obsolete]
    public interface IXmlTest{
        bool IsXml(string data, XmlTestDepth testDepth);
    }


    public static class transformExtensions{
        public static string transform(this string xml, string xslt, IDictionary parameters)
        {
            var trans = new XslCompiledTransform();
            trans.Load(xslt.asXPathNavigable(), XsltSettings.TrustedXslt, new FilePathXmlResolver(myapp.files));
            var args = XsltStandardExtension.PrepareArgs();
            if (null != parameters)
            {
                foreach (var parameter in parameters.Keys)
                {
                    var nameorns = parameter.ToString();
                    var paramorext = parameters[parameter];
                    bool ext = true;
                    if (paramorext is XPathNavigator || paramorext is XPathNavigator || paramorext is string || paramorext is ValueType)
                    {
                        ext = false;
                    }
                    if (ext)
                    {
                        args.AddExtensionObject(nameorns, paramorext);
                    }
                    else
                    {
                        args.AddParam(nameorns, string.Empty, paramorext);
                    }
                }
            }
            var sw = new StringWriter();
            trans.Transform(xml.asXPathNavigable(), args, sw);
            return sw.ToString();
        }
    }
}