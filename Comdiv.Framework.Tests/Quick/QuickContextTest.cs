using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Comdiv.Framework.Quick;
using NUnit.Framework;

namespace Comdiv.Framework.Tests.Quick
{
	[TestFixture]
	public class QuickContextTest
	{
		[Test]
		public void command_parsed() {
			Assert.AreEqual("start.end", new QuickContext("http://server/app/start/end.quick?param").Command);
			Assert.AreEqual("start.end", new QuickContext("http://server/app/start/end.json.quick?param").Command);
		}

		[Test]
		public void type_parsed()
		{
			Assert.AreEqual("json", new QuickContext("http://server/app/start/end.quick?param").Type);
			Assert.AreEqual("json", new QuickContext("http://server/app/start/end.json.quick?param").Type);
			Assert.AreEqual("wiki", new QuickContext("http://server/app/start/end.wiki.quick?param").Type);
		}

		[Test]
		public void works_with_httpcontext() {
			var req = new HttpRequest("", "http://server/app/start/end.wiki.quick?param", "x=1&y=2");
			var ctx = new HttpContext(req,new HttpResponse(null));
			var context = new QuickContext(ctx);
			Assert.AreEqual("start.end",context.Command);
			Assert.AreEqual("wiki", context.Type);
			Assert.AreEqual("1",context.Parameters["x"]);
			Assert.AreEqual("2", context.Parameters["y"]);
		}
	}

}
