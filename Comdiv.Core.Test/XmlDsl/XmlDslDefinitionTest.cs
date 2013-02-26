using System;
using System.Linq;
using System.Xml.Linq;
using Comdiv.Extensions;
using Comdiv.QWeb;
using Comdiv.Xml;
using NUnit.Framework;

namespace Comdiv.Core.Test.XmlDsl {
	[TestFixture]
	public class XmlDslDefinitionTest {
		private string code = @"
transform t1
transform_import t2
transform_import t3
transform_param x, 1
transform_param y, 'select://*'
transform_extension e1, 'http://e1' : 'Comdiv.Core.Test.XmlDsl.Testext1, Comdiv.Core.Test'
transform_extension e2, 'http://e2' : 'Comdiv.Core.Test.XmlDsl.Testext2, Comdiv.Core.Test'
";
		private XElement _src;
		private XmlDslDefinition _def;

		[SetUp]
		public void setup() {
			_src = MyBxl.Parse(code);
			_def = new XmlDslDefinition().Extract(_src);
		}
		

		[Test]
		public void can_embed_extension_namespaces() {
			var newxslt =
				_def.PrepareXslt(XElement.Parse(@"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>"));
			Console.WriteLine(newxslt.ToString());
			Assert.NotNull(newxslt.Attributes().FirstOrDefault(x=>x.Name=="{"+XNamespace.Xmlns+"}e1" && x.Value=="http://e1"));
			Assert.NotNull(newxslt.Attributes().FirstOrDefault(x => x.Name == "{" + XNamespace.Xmlns + "}e1" && x.Value == "http://e1"));
		}

		[Test]
		public void can_embed_extension_namespaces_existed_not_worry()
		{
			var newxslt =
				_def.PrepareXslt(XElement.Parse(@"<xsl:stylesheet version='1.0' xmlns:e1='http://e1' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>"));
			Console.WriteLine(newxslt.ToString());
			Assert.NotNull(newxslt.Attributes().FirstOrDefault(x => x.Name == "{" + XNamespace.Xmlns + "}e1" && x.Value == "http://e1"));
			Assert.NotNull(newxslt.Attributes().FirstOrDefault(x => x.Name == "{" + XNamespace.Xmlns + "}e1" && x.Value == "http://e1"));
		}

		[Test]
		public void can_embed_parameters() {
			var newxslt =
				_def.PrepareXslt(XElement.Parse(@"<xsl:stylesheet version='1.0' xmlns:e1='http://e1' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>"));
			Console.WriteLine(newxslt.ToString());
			var ps = newxslt.Elements().Where(x => x.Name.LocalName == "param").ToArray();
			Assert.AreEqual(2,ps.Length);
			Assert.AreEqual("x",ps[0].attr("name"));
			Assert.AreEqual("y",ps[1].attr("name"));
			Assert.AreEqual("1",ps[0].Value);
			Assert.AreEqual("//*",ps[1].attr("select"));
			
		}

		[Test]
		public void can_embed_extension_namespaces_existed_error_on_conflict()
		{
			var e = Assert.Throws<XmlDslLoaderException>(()=>_def.PrepareXslt(XElement.Parse(@"<xsl:stylesheet version='1.0' xmlns:e1='http://e2' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>")));
			Console.WriteLine(e.ToString());
		}

		[Test]
		public void lang()
		{
			Assert.AreEqual("t1", _def.LangName);
		}
		[Test]
		public void imports()
		{
			Assert.AreEqual(2, _def.Imports.Count);
			Assert.AreEqual("t2", _def.Imports[0]);
			Assert.AreEqual("t3", _def.Imports[1]);
		}
		[Test]
		public void parameters()
		{
			Assert.AreEqual(2, _def.Parameters.Count);
			Assert.AreEqual("1", _def.Parameters["x"]);
			Assert.AreEqual("select://*", _def.Parameters["y"]);
		}
		[Test]
		public void extensions()
		{
			Assert.AreEqual(2, _def.Extensions.Count);
			Assert.AreEqual("e1", _def.Extensions[0].Code);
			Assert.AreEqual("e2", _def.Extensions[1].Code);
			Assert.AreEqual("http://e1", _def.Extensions[0].NameSpace);
			Assert.AreEqual("http://e2", _def.Extensions[1].NameSpace);
			Assert.AreEqual(typeof(Testext1), _def.Extensions[0].Type);
			Assert.AreEqual(typeof(Testext2), _def.Extensions[1].Type);
			Assert.IsInstanceOf<Testext1>(_def.Extensions[0].Instance);
			Assert.IsInstanceOf<Testext2>(_def.Extensions[1].Instance);
		}

		[Test]
		public void generate_args()
		{
			var args = _def.CreateArguments();
			Assert.AreEqual("1", args.GetParam("x", ""));
			Assert.Null(args.GetParam("y", ""));
			Assert.IsInstanceOf<Testext1>(args.GetExtensionObject("http://e1"));
			Assert.IsInstanceOf<Testext2>(args.GetExtensionObject("http://e2"));
		}
	}
}