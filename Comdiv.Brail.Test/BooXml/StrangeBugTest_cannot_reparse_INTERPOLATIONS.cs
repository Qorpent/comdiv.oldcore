using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Booxml;
using Comdiv.QWeb.Serialization.BxlParser;
using NUnit.Framework;

namespace Comdiv.Brail.Test.BooXml
{
    [TestFixture]
    public class BUG_StrangeBugTest_cannot_reparse_INTERPOLATIONS
    {
        [Test]
        public void main_test() {
			var p = new BxlXmlParser();
            var x = p.Parse("c x='${y}' : '${z}'","main");
			Assert.AreEqual(@"<root><c _file=""main"" _line=""1"" x=""${y}"">${z}</c></root>", x.ToString(SaveOptions.DisableFormatting));
        }
		[Test]
		public void keep_single_usd_test()
		{
			var p = new BxlXmlParser();
			var x = p.Parse("c x='$' : '${z}'","main");
			Assert.AreEqual(@"<root><c _file=""main"" _line=""1"" x=""$"">${z}</c></root>", x.ToString(SaveOptions.DisableFormatting));
		}   
		[Test]
		public void bug_not_well_formed_usd() {
			var p = new BxlXmlParser();
			var x = p.Parse("col \"vUSD\", \"$\"","main");
			Assert.AreEqual(@"<root><col _file=""main"" _line=""1"" code=""vUSD"" id=""vUSD"" name=""$"" /></root>", x.ToString(SaveOptions.DisableFormatting));
			
		}
    }
}
