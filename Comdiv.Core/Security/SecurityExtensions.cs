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

using System.Collections.Generic;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model.Interfaces;
using Qorpent.Security;

//using Comdiv.QWeb;

namespace Comdiv.Security{
    ///<summary>
    ///</summary>
    public static class SecurityExtensions{
        public static object sync = new object();
        private static IPrincipalSource _principalSource;
        private static IInversionContainer _container;
        internal static string __GetRole(this object obj)
        {
            if (null == obj) return null;
            if (obj is IWithRole) return ((IWithRole)obj).Role;
            if (obj is IWithRoles) return ((IWithRoles)obj).Roles;
			//if (obj is IAction) return ActionAttribute.GetRole((IAction) obj);
            return null;
        }

        private static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (sync){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

       

        public static bool isForUser(this IWithRoles roles, IPrincipal principal = null ) {
            if(roles==null) return true;
            if(string.IsNullOrWhiteSpace(roles.Roles)) return true;
            principal = principal ?? myapp.usr;
            if(myapp.roles.IsAdmin(principal)) return true;
            foreach (var role in roles.Roles.split()) {
                if(myapp.roles.IsInRole(principal,role)) return true;
            }
            return false;
        }
        

        public static IPrincipal toPrincipal(this string username, params string[] roles){
            var un = UserName.For(username);
            return new GenericPrincipal(new GenericIdentity(un.LocalizedName),roles ?? new string[]{});
        }

        ///<summary>
        ///</summary>
        private static IPrincipalSource PrincipalSource{
            get{
                if (_principalSource == null){
                    _principalSource = Container.get<IPrincipalSource>() ?? new PrincipalSource();
                }
                return _principalSource;
            }
            set { _principalSource = value; }
        }


        private static IPrincipal getCurrent(){
            return PrincipalSource.CurrentUser;
        }

        public static bool IsAdmin(this IRoleResolver resolver){
            return IsAdmin(resolver, getCurrent());
        }

        public static bool IsAdmin(this IRoleResolver resolver, IPrincipal user){
            return resolver.IsInRole(user, "ADMIN");
        }

          public static bool IsInRole(this IRoleResolver resolver, string role, bool adminanyrole = true ){
            return resolver.IsInRole(getCurrent(), role,adminanyrole);
        }

        public static IEnumerable<string> GetRoles(this IRoleResolver resolver, bool donotproceedifadmin = true){
            if (null == resolver){
                return new string[]{};
            }
            return resolver.GetRoles(Qorpent.Applications.Application.Current.Principal.CurrentUser);
        }
    }
}