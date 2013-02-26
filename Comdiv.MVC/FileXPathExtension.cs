#region

using System;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

#endregion

namespace Comdiv.Web{
    [Obsolete]
    public static class FileXPathExtension{
        public static XPathNodeIterator Select(this String s, string xpath){
            var filename = HttpContext.Current.Server.MapPath(s);
            var doc = new XPathDocument(filename);
            return doc.CreateNavigator().Select(xpath);
        }
    }
}