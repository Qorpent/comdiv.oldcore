// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Logging;
using Comdiv.Security;
using Qorpent.Security;


namespace Comdiv.Security.Acl{
    public static class acl{
        public static object sync = new object();
        private static IInversionContainer _container;
        public static IInversionContainer Container
        {
            get
            {
                if (_container.invalid())
                {
                    lock (sync)
                    {
                        if (_container.invalid())
                        {
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        

        public const string accessPermission = "access";
        public const string readPermission = "read";
        public const string writePermission = "write";
        public const string executePermission = "execute";
        public static string token(object obj){
            var tokenresolver = Container.get<IAclTokenResolver>();
            if(null!=tokenresolver)return tokenresolver.GetToken(obj);
            return obj.ToString();
        }
        public static IAclProviderService provider{
            get { return Container.get<IAclProviderService>(); }
        }
        public static IAclRepository repository
        {
            get { return Container.get<IAclRepository>(); }
        }

        public static string token<T>(object id)
        {
            lock(sync)return token(typeof (T), id);
        }
        public static string token(Type type, object id){
            lock (sync) return Container.get<IAclTokenResolver>().GetToken(type, id == null ? String.Empty : id.ToString());
        }

        public static bool get<T>(string key){
            lock (sync)  return get<T>(key, false);
        }

        public static bool get<T>(string key,bool def){
            lock (sync) return get<T>(key, accessPermission, def);
        }

        public static bool get<T>(string key,string permission){
            lock (sync) return get<T>(key, permission, false);
        }

        public static bool get<T>(string key,string permission,bool def){
            lock (sync) return get<T>(key, permission, String.Empty, def);
        }

        public static bool get<T>(string key,string permission, string system){
            lock (sync) return get<T>(key, permission, system, false);
        }

        public static bool get<T>(string key,string permission, string system,bool def){
            lock (sync) return get<T>(key, permission, system, Container.get<IPrincipalSource>().CurrentUser, def);
        }

        public static bool get<T>(string key,string permission, string system, IPrincipal principal){
            lock (sync) return get<T>(key, permission, system, principal, false);
        }

        public static bool get<T>(string key,string permission, string system, IPrincipal principal,bool def){
            lock (sync) return get(token<T>(key), permission, system, principal, def);
        }

        public static bool get(string token){
            lock (sync) return get(token, false);
        }

        public static bool get(string token,bool def){
            lock (sync) return get(token, accessPermission, def);
        }

        public static bool get(string token,string permission){
            lock (sync) return get(token, permission, false);
        }

        public static bool get(string token,string permission,bool def){
            lock (sync) return get(token, permission, String.Empty, def);
        }

        public static bool get(string token,string permission, string system){
            lock (sync) return get(token, permission, system, false);
        }

        public static bool get(string token,string permission, string system,bool def){
            lock (sync) return get(token, permission, system, Container.get<IPrincipalSource>().CurrentUser, def);
        }

        public static bool get(string token,string permission, string system, IPrincipal principal){
            lock (sync) return get(token, permission, system, principal, false);
        }


        public static bool get(object obj)
        {
            lock (sync) return get(obj, false);
        }

        public static bool get(object obj, bool def)
        {
            lock (sync) return get(obj, accessPermission, def);
        }

        public static bool get(object obj, string permission)
        {
            lock (sync) return get(obj, permission, false);
        }

        public static bool get(object obj, string permission, bool def)
        {
            lock (sync) return get(obj, permission, String.Empty, def);
        }

        public static bool get(object obj, string permission, string system)
        {
            lock (sync) return get(obj, permission, system, false);
        }

        public static bool get(object obj, string permission, string system, bool def)
        {
            lock (sync) return get(obj, permission, system, Container.get<IPrincipalSource>().CurrentUser, def);
        }

        public static bool get(object obj, string permission, string system, string usr){
            return get(obj, permission, system, new GenericPrincipal(new GenericIdentity(usr), new string[] { }));
        }

        public static bool get(object obj, string permission, string system, IPrincipal principal)
        {
            lock (sync) return get(obj, permission, system, principal, false);
        }

        public static bool get(object obj,string permission, string system, string usr, bool def){
            return get(obj, permission, system, new GenericPrincipal(new GenericIdentity(usr),new string[]{}), def);
        }

        public static bool get(object obj,string permission, string system, IPrincipal principal,bool def){
            
            lock (sync) {
                var rname = obj.__GetRole();
                if(rname.hasContent()){
                    if("DEFAULT"==rname) return true;
                    var roles = Container.get<IRoleResolver>();
                    if (roles.IsInRole(principal, "ADMIN"))
                    {
                        return true;
                    }
                    var myroles = rname.split(false,true,',','|');
					if (rname.Contains("|"))
					{
						foreach (var role in myroles)
						{
							if (role == "DEFAULT" || role == "PUBLIC") return true;
							if (roles.IsInRole(principal, role))
							{
								return true;

							}
							
						}
						return false;
					}
					else {
						foreach (var role in myroles) {
							if (role == "DEFAULT" || role == "PUBLIC") continue;
							if (roles.IsInRole(principal, role)) {
								continue;

							}
							return false;
						}
					}
                	return true;
                }
                return get(token(obj), permission, system, principal, def);
            }
        }
        public static bool get(string token,string permission, string system, IPrincipal principal,bool def){
            lock (sync){
                if (Container.get<IRoleResolver>().IsInRole(principal, "ADMIN")){
                    return true;
                }
                var e = Container.get<IAclProviderService>();
                if(null==e) return def;
                var result =
                    e.Evaluate(new AclRequest{
                                                                                    Permission = permission,
                                                                                    Principal = principal,
                                                                                    System = system,
                                                                                    Token = token
                                                                                });
                bool _result = def;
                if (result.Processed) _result = result.AccessAllowed;
                log.Info(_result + "=" + principal.Identity.Name + ":" + token + "?" + permission + "/" + system + "/");
                return _result;
            }
        }


        private static readonly ILog log = logger.get("comdiv.security.acl");
    }
}