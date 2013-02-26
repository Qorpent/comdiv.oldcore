using System;
using System.IO;
using System.Xml;

namespace Comdiv.Xml {
	/// <summary>
	/// url resolver for prepared XSLT (must keep root path)
	/// </summary>
	public class XmlDslRootBasedXmlUrlResolver :XmlUrlResolver {
		private Uri _baseuri;

		/// <summary>
		/// creates new instance
		/// </summary>
		/// <param name="file"></param>
		public XmlDslRootBasedXmlUrlResolver(string file) {
			_baseuri = new Uri(file);
		}

		public override Uri ResolveUri(Uri baseUri, string relativeUri)
		{
			return base.ResolveUri(_baseuri, relativeUri);
		}
		
	}
}