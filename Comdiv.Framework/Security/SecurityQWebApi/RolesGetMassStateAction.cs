using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Comdiv.Extensions;
using Qorpent.Mvc;
namespace Comdiv.Framework.Security.SecurityQWebApi {
	[Action("roles.getmassstate", Role = "ADMIN")]
	public class RolesGetMassStateAction : RolesActionBase
	{

		protected override object MainProcess() {
			var result = new List<object>();
			var targets = this.matrix.XPathSelectElements("//targets/target");
				foreach (var xt in targets) {
					var file = ResolveFileName( xt.attr("file") );
					var applyer = RoleApplyer.CreateByFileName(file);
					var target = xt.attr("code");
					var maps = applyer.GetAllUserRoleMaps(file);
					foreach (var um in maps) {
						result.Add(new { id = string.Format("{0}_{1}_{2}", um.User.Replace("\\","/"), target, um.Role), value = true });
					}
				}
			
			return result.ToArray();
		}

	}
}