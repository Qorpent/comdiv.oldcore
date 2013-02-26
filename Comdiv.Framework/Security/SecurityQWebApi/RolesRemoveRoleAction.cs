using Qorpent.Mvc;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	[Action("roles.removerole", Role = "ADMIN")]
	public class RolesRemoveRoleAction : RolesActionBase
	{
		protected override object MainProcess()
		{
			applyer.Remove(file,user,role);
			return base.MainProcess();
		}
	}
}