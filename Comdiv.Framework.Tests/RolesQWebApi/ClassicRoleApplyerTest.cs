using System;
using System.IO;
using Comdiv.Framework.Security.SecurityQWebApi;
using NUnit.Framework;

namespace Comdiv.Framework.Tests.RolesQWebApi {
	[TestFixture]
	public class ClassicRoleApplyerTest {
		private string file;

		[SetUp]
		public void setup() {
			this.file = Path.GetTempFileName()+".config";
			
		}
		[TearDown]
		public void teardown() {
			File.Delete(this.file);
		}

		[Test]
		public void can_add_role_mappings() {
			var a = new ClassicXmlRoleApplyer();
			a.Add(file,@"d\u","ADMIN");
			a.Add(file, @"d2\u2", "ADMIN2");
			Console.WriteLine(File.ReadAllText(file));
			Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-8""?>
<config>
  <map user=""d\u"" as=""ADMIN"" />
  <map user=""d2\u2"" as=""ADMIN2"" />
</config>",File.ReadAllText(file));
		}

		[Test]
		public void can_remove_role_mappings() {
			var a = new ClassicXmlRoleApplyer();
			a.Add(file,@"d\u","ADMIN");
			a.Add(file, @"d2\u2", "ADMIN2");
			a.Remove(file, @"d2\u2", "ADMIN2");
			a.Remove(file, @"d2\u2", "ADMIN3");
			Console.WriteLine(File.ReadAllText(file));
			Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-8""?>
<config>
  <map user=""d\u"" as=""ADMIN"" />
</config>",File.ReadAllText(file));
		}
	}
}