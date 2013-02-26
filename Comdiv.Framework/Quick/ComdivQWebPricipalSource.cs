using System;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Security;
using Qorpent.Security;

namespace Comdiv.Framework.Quick {
	public class ComdivQWebPricipalSource: IPrincipalSource {
		public IPrincipal CurrentUser {
			get { return myapp.usr; }
		}

		/// <summary>
		///  Base principal used before impersonation
		/// </summary>
		public IPrincipal BasePrincipal {
			get { return myapp.principals.BasePrincipal; }
			set { myapp.principals.BasePrincipal = value; }
		}

		public void SetCurrentUser(IPrincipal logonUser) {
			PrincipalSource._current = logonUser;
		}
	}
}