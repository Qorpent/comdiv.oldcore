using Qorpent.Mvc;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	[Action("roles.getstate", Role = "ADMIN")]
	public class RolesGetStateAction : RolesActionBase
	{

		protected override object MainProcess()
		{
			return applyer.Exists(file, user, role);
		}
	}
}