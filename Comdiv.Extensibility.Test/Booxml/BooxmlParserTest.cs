//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Comdiv.QWeb.Serialization.BxlParser;
using NUnit.Framework;

namespace Comdiv.Booxml.Test {
	[TestFixture]
	public class Boo_And_QWeb_BXL_Parser_Comparation {
		[Test]
		[Ignore("now real bxl's written on incompatible manner")]
		public void long_boo_bxl_test() {

			var c = File.ReadAllText(@"C:\code\usr\ugmk\eco\data\eco.real.bxl");
			var s = Stopwatch.StartNew();
			XElement x = null;
			for(int i=0;i<30;i++) {
				x = new BooxmlParser().Parse(c);
			}
			Console.WriteLine(x.ToString().Length);
			File.WriteAllText(@"c:\tmp\usual_bxl.xml",x.ToString());
		}
		[Test]
		[Ignore]
		public void long_qweb_bxl_test()
		{

			var c = File.ReadAllText(@"C:\code\usr\ugmk\eco\data\eco.real.bxl");
			var s = Stopwatch.StartNew();
			XElement x = null;
			for (int i = 0; i < 30; i++)
			{
				x = new BxlXmlParser().Parse(c, "main");
			}
			Console.WriteLine(x.ToString().Length);
			File.WriteAllText(@"c:\tmp\qweb_bxl.xml", x.ToString());
		}

		[Test]
		[Ignore("now real bxl's written on incompatible manner")]
		public void long_qweb_bxl_test_BUG()
		{

			var c = File.ReadAllText(@"C:\code\usr\ugmk\eco\data\eco.param.bxl");
			var s = Stopwatch.StartNew();
			XElement x = null;
			for (int i = 0; i < 1; i++)
			{
				x = new BxlXmlParser().Parse(c, "main");
			}
			Console.WriteLine(x.ToString().Length);
			File.WriteAllText(@"c:\tmp\qweb_param_bxl.xml", x.ToString());
		}
	}

    [TestFixture]
    public class BooxmlParserTest : BooxmlBaseTest {
        [Test]
        public void all_defaults__provided() {
            Assert.AreEqual("<root><e1 id='x' code='x' name='y' fix='1' /></root>", test_(@"
e1 x,y,fix
"));
        }


        [Test]
        public void attribute_setting() {
            Assert.AreEqual("<root><e1 x='1'><e2 x='3' y='4' /></e1></root>",
                            test_(@" 
e1 x=1:
    e2 x=3 :
        y=4
"));
        }

        [Test]
        public void attribute_specialname_setting() {
            Assert.AreEqual("<root><e1 x.bool='1' /></root>", test_(@"
e1 x.bool=1        
"));
        }

        [Test]
        public void embed_embeds() {
            Assert.AreEqual("<root><e1 x='${x}' /></root>", test_(@" 
e1 x='${x}'
"));
        }

        [Test]
        public void embed_embeds_2() {
            Assert.AreEqual("<root><e1 x='${x}' /></root>", test_(@" 
e1 x=""${x}""
"));
        }

        [Test]
        public void embed_embeds_3() {
            Assert.AreEqual(@"<root><e1>${x}</e1></root>", test_(@" 
e1 : ""${x}""
"));
        }

        [Test]
        public void html_expansion() {
            var x =
                test_(@"
e1 x=1:
    e2 : """"""
        <html>
            <br/>
        </html>
    """"""
");
            Assert.AreEqual(
                @"<root><e1 x='1'><e2><![CDATA[<html>
            <br/>
        </html>]]></e2></e1></root>", x);
        }

        [Test]
        public void id_attribute_provided() {
            Assert.AreEqual(
                "<root><e1 id='x' code='x'><e2 id='x2' code='x2' /><e3 id='x.name' code='x.name' /></e1></root>",
                test_(@"
e1 x:
    e2 'x2'
    e3 x.name
"));
        }


        [Test]
        public void name_attribute_provided() {
            Assert.AreEqual("<root><e1 id='x' code='x' name='y' /></root>", test_(@"
e1 x,y
"));
        }

        [Test]
        public void namespace_became_root() {
            Assert.AreEqual("x", test("namespace x").Name.LocalName);
        }

        [Test]
        public void null_namespace_cause_root_root() {
            Assert.AreEqual("root", test("z").Name.LocalName);
        }

        [Test]
        public void single_element_in_root() {
            Assert.AreEqual("<root><element /></root>", test_("element"));
        }

        [Test]
        public void strange_bug() {
            test(
                @"
eco:
    imports nofiles
    x=1
    
	
");
        }

        [Test]
        public void text_expansion() {
            Assert.AreEqual("<root><e1 x='1'><e2 y.x='4'>x2\r\ny2</e2><e3>x3</e3></e1></root>",
                            test_(@"
e1 x=1:
    e2 : 
        'x2'
        y.x=4
        'y2'
    e3 : 'x3'
"));
        }

        [Test]
        public void text_expansion_on_negation() {
            Assert.AreEqual("<root><e1>-1</e1></root>", test_(@"
e1:-1
"));
        }

        [Test]
        public void wasbug_in_parsing() {
            test(
                @"
thema contacts :
	this.visible=false
	out ""-orgtel"", ""Телефонный справочник предприятия"" :
		role=DEFAULT
		template=empty
		param currentObject, ""Предприятие"", type=int, mode=template
		param viewname : ""contacts/orgtel""
		param generatorname : ""contacts/orgtel""");
        }

        [Test]
        public void xml_tree() {
            Assert.AreEqual("<root><e1><e2 /><e3 /></e1></root>", test_("e1:\r\n\te2\r\n\te3"));
        }
    }
}