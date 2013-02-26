using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Framework.Security.SecurityQWebApi;
using NUnit.Framework;

namespace Comdiv.Framework.Tests.RolesQWebApi
{
	[TestFixture]
	public class RoleApplyerTest
	{
		[Test]
		public void resolves_bxapplyer_correctly() {
			Assert.IsInstanceOf(typeof(BxlRoleApplyer),RoleApplyer.CreateByFileName(@"c:\folder\security.map.bxl"));
		}
		[Test]
		public void resolves_classicapplyer_correctly()
		{
			Assert.IsInstanceOf(typeof(ClassicXmlRoleApplyer), RoleApplyer.CreateByFileName(@"c:\folder\security.map.config"));
		}
		[Test]
		public void not_resolves_unknown_files() {
			Assert.Throws<RolesActionException>(() => RoleApplyer.CreateByFileName(@"c:\folder\security.map.unkfile"));
		}
	}
}
