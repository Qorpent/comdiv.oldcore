using System;
using System.IO;
using Comdiv.Framework.Security.SecurityQWebApi;
using NUnit.Framework;

namespace Comdiv.Framework.Tests.RolesQWebApi {
	[TestFixture]
	public class BxlRoleApplyerTest
	{
		private string file;

		[SetUp]
		public void setup()
		{
			this.file = Path.GetTempFileName() + ".bxl";

		}
		[TearDown]
		public void teardown()
		{
			File.Delete(this.file);
		}

		[Test]
		public void can_add_role_mappings()
		{
			var a = new BxlRoleApplyer();
			a.Add(file, @"d\u", "ADMIN");
			a.Add(file, @"d2\u2", "ADMIN2");
			Console.WriteLine(File.ReadAllText(file));
			Assert.AreEqual(@"
map ""d/u"", ADMIN
map ""d2/u2"", ADMIN2", File.ReadAllText(file));
		}

		[Test]
		public void can_remove_role_mappings()
		{
			var a = new BxlRoleApplyer();
			a.Add(file, @"d\u", "ADMIN");
			a.Add(file, @"d2\u2", "ADMIN2");
			a.Remove(file, @"d2\u2", "ADMIN2");
			a.Remove(file, @"d2\u2", "ADMIN3");
			Console.WriteLine(File.ReadAllText(file));
			Assert.AreEqual(@"
map ""d/u"", ADMIN", File.ReadAllText(file));
		}
	}
}