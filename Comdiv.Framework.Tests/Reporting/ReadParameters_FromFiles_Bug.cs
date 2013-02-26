using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using Comdiv.Reporting;
using NUnit.Framework;
using Qorpent.Bxl;

namespace Comdiv.Framework.Tests.Reporting
{
	[TestFixture]
	public class ReadParameters_FromFiles_Bug
	{
		[Test]
		public void NoFind_File_Typed_Parameter_In_File() {
			var xml = new BxlParser().Parse(@"param type=file : test");
			var param = xml.read<Parameter>("//param").First();
			Assert.AreEqual("file", param.Type);
		}
	}
}
