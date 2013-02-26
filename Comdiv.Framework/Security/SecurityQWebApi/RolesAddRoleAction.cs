using Qorpent.Mvc;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	[Action("roles.addrole", Role = "ADMIN")]
	public class RolesAddRoleAction : RolesActionBase
	{

		protected override object MainProcess()
		{
			applyer.Add(file, user, role);
			return base.MainProcess();
		}
	}
}