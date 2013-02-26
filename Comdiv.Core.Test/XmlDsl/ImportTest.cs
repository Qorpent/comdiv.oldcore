using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Comdiv.IO;
using Comdiv.QWeb;
using Comdiv.Xml;
using NUnit.Framework;

namespace Comdiv.Core.Test.XmlDsl
{
	[TestFixture]
	public class ImportTest
	{
		[Test]
		public void native_can_load_usually() {
			new XslCompiledTransform().Load("./dsl/importtest.xslt",XsltSettings.TrustedXslt,new XmlUrlResolver());
		}

		[Test]
		public void XmlDslResolver_can_load_usually() {
			var x = XElement.Load("./dsl/importtest.xslt");
			new XslCompiledTransform().Load(x.CreateReader(), XsltSettings.TrustedXslt, new XmlDslRootBasedXmlUrlResolver(Path.GetFullPath("./dsl/importtest.xslt")));
		}

		[Test]
		public void no_error_in_XmlDsl_context_with_no_overrides() {
			var src = MyBxl.Parse(@"
transform importtest
");
			var result = new XmlDslLoader("~/dsl",new DefaultFilePathResolver()).Load(src);
			Assert.AreEqual("<test1 />",result.ToString());
		}

		[Test]
		public void no_error_in_XmlDsl_context_with_no_import_overrides()
		{
			var src = MyBxl.Parse(@"
transform importtest
transform_param t1, 245
");
			var result = new XmlDslLoader("~/dsl",new DefaultFilePathResolver()).Load(src);
			Assert.AreEqual("<test1 />", result.ToString());
		}


		[Test]
		public void CanEmbedCustomImport()
		{
			var src = MyBxl.Parse(@"
transform importtest
transform_import importtest-import2.xslt
");
			var result = new XmlDslLoader("~/dsl",new DefaultFilePathResolver()).Load(src);
			Assert.AreEqual("<test2 />", result.ToString());
		}
	}
}
