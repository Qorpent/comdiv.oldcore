using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Comdiv.Application;
using Comdiv.IO;


namespace Comdiv.Xml
{
	

	/// <summary>
	/// Loads BXL file with auto-execute xml transformation files
	/// to describe DSL in source u have to use:
	/// <code>
	/// transform {LANGCODE}									# transform file to load
	/// transform_import {FILENAME}								# custom imports (will be added to transformation XSLT)
	/// transform_extension {CODE}, {NAMESPACE} : {TYPENAME}    # transform extensions object (ns will be added with given prefix to XSLT)
	/// transform_param {NAME}, {VALUE}							# transform parameters (will be added as param to transfrom file)
	/// </code>
	/// transform elements will be removed from source 
	/// </summary>
	public class XmlDslLoader {
		/// <summary>
		/// creates new xmldsl loader with given folder of transformations
		/// </summary>
		/// <param name="rootdir"></param>
		/// <param name="resolver"> </param>
		public XmlDslLoader(string rootdir = "dsl", IFilePathResolver  resolver= null ) {
			Resolver = resolver ?? myapp.files;
			Rootdir = rootdir;	
		}

		/// <summary>
		/// File resolver
		/// </summary>
		public IFilePathResolver Resolver { get; set; }


		/// <summary>
		/// Converts source XML with given transform with applying DSL
		/// </summary>
		/// <param name="xml"> </param>
		/// <returns>XElement with data, converted by transform</returns>
		/// <exception cref="XmlDslLoaderException">
		/// <list type="bullet">
		/// <item><description>cannot find transform file</description></item>
		/// <item><description>any problem with underlined XmlDslDefinition</description></item>
		/// </list>
		/// </exception>
		public XElement Load(XElement xml) {
			if (null == xml.Element("transform")) return xml; //no dsl needed
			var trdef = new XmlDslDefinition().Extract(xml);
			var langfile = Resolver.Resolve(Rootdir+"/" + trdef.LangName + ".xslt", false);
			if (!File.Exists(langfile))
			{
				throw new XmlDslLoaderException("Не возможно найти файл XML языка" + langfile);
			}
			var xsl = new XslCompiledTransform();
			if(trdef.NeedPrepareXslt) {
				var xsltcontent = XElement.Load(langfile);
				xsltcontent = trdef.PrepareXslt(xsltcontent);
				var resolver = new XmlDslRootBasedXmlUrlResolver(langfile);
				xsl.Load(xsltcontent.CreateReader(),XsltSettings.TrustedXslt,resolver);
			}else {
				xsl.Load(langfile, XsltSettings.TrustedXslt, new XmlUrlResolver());
			}
			var args = trdef.CreateArguments();
			var sw = new StringWriter();
			using (var xw = XmlWriter.Create(sw)) {
				xsl.Transform(xml.CreateReader(),args,xw);
				xw.Flush();
			}
			return XElement.Parse(sw.ToString());
		}

		/// <summary>
		/// directory for transform files
		/// </summary>
		public string Rootdir { get; set; }
	}
}
