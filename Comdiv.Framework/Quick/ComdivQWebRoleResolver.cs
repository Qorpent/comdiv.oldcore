using System;
using System.Collections.Generic;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Security;
using Qorpent.Mvc;
using Qorpent.Security;

namespace Comdiv.Framework.Quick {
	public class ComdivQWebRoleResolver :  IRoleResolver
	{


		/// <summary>
		/// 	Test given principal against role
		/// </summary>
		/// <param name="principal"> </param>
		/// <param name="role"> </param>
		/// <param name="exact"> </param>
		/// <param name="callcontext"> </param>
		/// <param name="customcontext"> </param>
		/// <returns> </returns>
		public bool IsInRole(IPrincipal principal, string role, bool exact = false, IMvcContext callcontext = null, object customcontext = null) {
			var p = principal;
			if (principal is WindowsPrincipal)
			{
				if (principal.Identity.Name.toDomain().ToLower() == Environment.MachineName.ToLower())
				{
					p = ("local\\" + principal.Identity.Name.toUserName(false)).toPrincipal();
				}
			}
			return myapp.roles.IsInRole(p, role, exact);
		}

		/// <summary>
		/// 	Test given username against role
		/// </summary>
		/// <param name="username"> </param>
		/// <param name="role"> </param>
		/// <param name="exact"> </param>
		/// <param name="callcontext"> </param>
		/// <param name="customcontext"> </param>
		/// <returns> </returns>
		public bool IsInRole(string username, string role, bool exact = false, IMvcContext callcontext = null, object customcontext = null) {
			return IsInRole(new GenericPrincipal(new GenericIdentity(username), null), role, exact, callcontext, customcontext);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="principal"></param>
		/// <param name="callcontext"> </param>
		/// <param name="customcontext"> </param>
		/// <returns></returns>
		public IEnumerable<string> GetRoles(IPrincipal principal, IMvcContext callcontext = null, object customcontext = null) {
			return myapp.roles.GetRoles(principal, callcontext, customcontext);
		}
	}
}