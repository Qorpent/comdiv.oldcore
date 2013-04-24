// Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//  Supported by Media Technology LTD 
//   
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Logging;
using Qorpent.Mvc;
using Qorpent.Security;

namespace Comdiv.Security{

    public class RoleResolver : IRoleResolver, IWithContainer{
        private IInversionContainer _container;
        private IEnumerable<string> allroles;
        protected IDictionary<string, bool> cache = new Dictionary<string, bool>();
        public object lockSync = new object();
        public ILog log = logger.get("comdiv.security.roleresolver");

        private bool usewindows = false;

        public RoleResolver(){
            myapp.OnReload += (s, a) => Reload();
            Reload();
        }

        protected List<IRoleResolverExtension> Extensions { get; set; }

        #region IRoleResolver Members

        public bool IsInRole(IPrincipal principal, string role, bool adminisanyrole = true){
            lock (this) {
	            if (string.IsNullOrWhiteSpace(role)) return true;
				if (null == principal) {
					principal = myapp.principals.CurrentUser;
				}
                if (log.IsDebugEnabled){
                    log.Debug("start check role " + role + " for " + principal.Identity.Name);
                }
                bool result;
                try{
                    result = _isinrole(role, principal, adminisanyrole);
                }
                catch (Exception ex){
                    log.Error("error while role resolving", ex);
                    throw;
                }
                if (log.IsDebugEnabled){
                    log.Debug("stop check role " + role + " for " + principal.Identity.Name + " result - " + result);
                }
                return result;
            }
        }

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
		    return IsInRole(principal, role, !exact);
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
		    return IsInRole(new GenericPrincipal(new GenericIdentity(username), null), role, !exact, callcontext, customcontext);
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="principal"></param>
	    /// <param name="callcontext"> </param>
	    /// <param name="customcontext"> </param>
	    /// <returns></returns>
	    public IEnumerable<string> GetRoles(IPrincipal principal, IMvcContext callcontext = null, object customcontext = null) {
		    return GetRoles(principal, true, callcontext, customcontext);
	    }

	    public IEnumerable<string> GetRoles(IPrincipal principal, bool donotproceedifadmin = true, IMvcContext callcontext = null, object customcontext = null)
		{
            lock (this){
                return internalGetRoles(principal, donotproceedifadmin).ToArray();
            }
        }

        #endregion

        #region IWithContainer Members

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        #endregion

        public RoleResolver Assign(IPrincipal principal, string role){
            var key1 = GetKey(role, principal, true);
            var key2 = GetKey(role, principal, false);
            cache[key1] = true;
            cache[key2] = true;
            return this;
        }

        private bool _isinrole(string role, IPrincipal principal, bool adminisanyrole = true){
            role = role.ToUpper();
            if(principal.Identity.Name=="local\\comdivsu") {
                return true;
            }
            if ("DEFAULT" == role){
                return true;
            }
            if("IMPERSONATED" == role) {
                return myapp.Impersonator.IsImpersonated(myapp.principals.BasePrincipal);
            }
            if (adminisanyrole && role != "ADMIN"){
                if (_isinrole("ADMIN", principal)){
                    return true;
                }
            }
            var key = GetKey(role, principal, adminisanyrole);
            return cache.get(key, () =>{
                                          var result = internalIsInRole(principal, role);
                                          if (result){
                                              return true;
                                          }

                                          return false;
                                      });
        }

        private static string GetKey(string role, IPrincipal principal, bool adminisanyrole){
            return (principal.Identity.Name + "_" + role + "_" + adminisanyrole).ToUpper();
        }

        private IEnumerable<string> internalGetRoles(IPrincipal principal, bool donotproceedifadmin){
            foreach (var role in internalGetActive(principal, donotproceedifadmin)){
                if (role == "DEFAULT"){
                    continue;
                }

                yield return role;
            }
            //always return at end
            yield return "DEFAULT";
        }


        public void Reload(){
            lock (this){
                cache.Clear();
                allroles = null;
                usewindows = false;
                Extensions = Container.all<IRoleResolverExtension>().ToList();
            }
        }


        protected virtual IEnumerable<string> internalGetActive(IPrincipal principal, bool donotproceedifadmin){
            if (donotproceedifadmin && IsInRole(principal, "ADMIN",adminisanyrole:true)){
                return new[]{"ADMIN"};
            }
            return getallRoles().Where(role => IsInRole(principal, role, false));
        }

        private IEnumerable<string> getallRoles(){
            lock (this){
                return allroles ?? (allroles = _getAllRoles().Distinct());
            }
        }

        private IEnumerable<string> _getAllRoles(){
            yield return "ADMIN";
            yield return "DESIGNER";
            yield return "DEFAULT";
            foreach (var extension in Extensions){
                foreach (var role in extension.GetRoles()){
                    yield return role;
                }
            }
        }


        protected virtual bool internalIsInRole(IPrincipal principal, string role){
            var dolog = log.IsDebugEnabled;
            if (dolog){
                log.Debug("try by principal self method");
            }
            // very strange code around windows, but it's neccesary
            // because strange problems with domains
            try{
                if (usewindows || !(principal is WindowsPrincipal)){
                    if (principal.IsInRole(role)){
                        return true;
                    }
                }
            }
            catch (SystemException){
                if (principal is WindowsPrincipal){
                    usewindows = false;
                }
                else{
                    throw;
                }
            }

			if(role=="ADMIN") {
				var exclusive = Extensions.FirstOrDefault(x => x.IsExclusiveSuProvider);
				if(null!=exclusive) {
					return exclusive.IsInRole(principal, "ADMIN");
				}
				
			}

            if (Extensions.Any(extension => extension.IsInRole(principal, role))){
                return true;
            }
            if (dolog){
                log.Debug("role was not recognized for user for all methods");
            }
            return false;
        }

    }
}