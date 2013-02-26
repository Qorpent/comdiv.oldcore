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
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Web;
using Comdiv.Design;
using Comdiv.Extensions;
using Qorpent.Security;

namespace Comdiv.Security{
    ///<summary>
    ///</summary>
    public class PrincipalSource : IPrincipalSource{
        [ThreadStatic] public static IPrincipal _current;
        [ThreadStatic] public static DateTime _stamp;
        public object lockSync = new object();


        public IImpersonator Impersonator { get; set; }

        #region IPrincipalSource Members

        public IPrincipal CurrentUser{
            get{
                lock (lockSync){
                    checkThread();
                    if (Impersonator != null && Impersonator.Active){
                        return Impersonator.Resolve(_current);
                    }
                    return _current;
                }
            }
        }

	    /// <summary>
	    /// 	Manually set current user for thread
	    /// </summary>
	    /// <param name="usr"> </param>
	    public void SetCurrentUser(IPrincipal usr) {
		    _current = usr;
	    }

	    public IPrincipal BasePrincipal{
            get{
                lock (lockSync){
                    checkThread();
                    return _current;
                }
            }
            set{
                lock (lockSync){
                    _current = value;
                }
            }
        }

        #endregion

        private void checkThread(){
            if (_current != null && HttpContext.Current != null){
                if (HttpContext.Current.Timestamp != _stamp){
                    _current = null;
                    _stamp = HttpContext.Current.Timestamp;
                }
            }
            if (null == _current){
                _current =
                    getHttpUser()
                    ??
                    Thread.CurrentPrincipal;
                var un = UserName.For(_current);
                if (un.IsLocal && un.Domain!="local") {
                	var n = un.Name;
					if (n.noContent()) {
						var domain = Environment.UserDomainName;
						if(domain.ToLower()==Environment.MachineName.ToLower()) {
							domain = "local";
						}
						_current = (domain + "\\" + Environment.UserName).toPrincipal();
					}
					else {
						_current = (@"local\" + un.Name).toPrincipal();
					}
                }
            }
        }


        [NoCover("cannot reproduce behaviour of httpcontext.current in test environment")]
        protected IPrincipal getHttpUser(){
            if (HttpContext.Current == null){
                return null;
            }
            if (HttpContext.Current.User == null){
                return new GenericPrincipal(new GenericIdentity("STARTUP_APP_USER"), new[]{"ADMIN"});
            }
           
			/*if (string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name)){
                return new GenericPrincipal(new GenericIdentity("GUEST"), new[]{"DEFAULT"});
            }*/

            return HttpContext.Current.User;
        }
    }
}