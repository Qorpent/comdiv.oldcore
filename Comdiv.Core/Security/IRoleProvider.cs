 // COPYRIGHT 2007-2011 COMDIV (F. SADYKOV) - HTTP://CODE.GOOGLE.COM/U/FAGIM.SADYKOV/
 // SUPPORTED BY MEDIA TECHNOLOGY LTD 
 //  
 // LICENSED UNDER THE APACHE LICENSE, VERSION 2.0 (THE "LICENSE");
 // YOU MAY NOT USE THIS FILE EXCEPT IN COMPLIANCE WITH THE LICENSE.
 // YOU MAY OBTAIN A COPY OF THE LICENSE AT
 //  
 //      HTTP://WWW.APACHE.ORG/LICENSES/LICENSE-2.0
 //  
 // UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE
 // DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS,
 // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED.
 // SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING PERMISSIONS AND
 // LIMITATIONS UNDER THE LICENSE.
 // 
 // MODIFICATIONS HAVE BEEN MADE TO THIS FILE

using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Comdiv.Security{
    /// <summary>
    /// Interface for RoleResolver - to provide extended resolvers for different sources
    /// </summary>
    [Obsolete("переход на Qorpent",true)]
	public interface IRoleProvider{
        /// <summary>
        /// Simple method near to IRoleResolver, works as ADMINBEHAVIOUR==false for
        /// direct role resolving
        /// </summary>
        /// <param name="principal">principal to check</param>
        /// <param name="role">role to check</param>
        /// <returns>true if principal is in role</returns>
        bool IsInRole(IPrincipal principal, string role);
        /// <summary>
        /// Returns registered roles of this extension
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetRoles();

	    /// <summary>
	    /// 
	    /// </summary>
	    bool IsExclusiveSuProvider { get; }
    }
}