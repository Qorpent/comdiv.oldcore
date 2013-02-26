using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Persistence;
using Qorpent.IoC;
using Qorpent.Mvc;
using Qorpent.Security;
using Enumerable = System.Linq.Enumerable;

namespace Comdiv.Security {
    ///<summary>
    ///</summary>
    [ContainerComponent(Lifestyle.Extension,ServiceType = typeof(IRoleResolverExtension),Name = "persistent.roles.extension")]
	public class PersistentRoleProvider : IRoleResolverExtension {

        private StorageWrapper<IRoleResolverRecord> storage;

        public PersistentRoleProvider()
        {
            storage = myapp.storage.Get<IRoleResolverRecord>(false);
        }



	    /// <param name="principal"> </param>
	    /// <param name="role"> </param>
	    /// <param name="exact"> </param>
	    /// <param name="callcontext"> </param>
	    /// <param name="customcontext"> </param>
	    /// <returns> </returns>
	    public bool IsInRole(IPrincipal principal, string role, bool exact = false, IMvcContext callcontext = null, object customcontext = null) {
			if (null != storage)
			{
				try
				{
					var name = principal.Identity.Name;
					var mylogin = storage.First("? in (Login,Login2)", name);

					if (null != mylogin)
					{
						return mylogin.Active &&
							   Enumerable.Contains(mylogin.Roles.split().Select(r => r.ToUpper()), role.ToUpper());
					}
				}
				catch (Exception ex)
				{
					//   CoreLogger.Common.Error("Ошибка при разрешении ролей",ex);
				}
			}
			return false;
	    }

	    public IEnumerable<string> GetRoles() {
            return new string[] {};
        }

	    /// <summary>
	    /// 
	    /// </summary>
	    public bool IsExclusiveSuProvider { get { return false; }}

	    /// <summary>
	    /// 	An index of object
	    /// </summary>
	    public int Idx { get; set; }
    }
}