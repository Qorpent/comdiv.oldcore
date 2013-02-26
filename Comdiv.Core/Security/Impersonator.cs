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
using Comdiv.Inversion;
using Qorpent.Security;

namespace Comdiv.Security{
    public class Impersonator : IImpersonator{
        private static readonly IDictionary<string, IPrincipal> impersonation = new Dictionary<string, IPrincipal>();
        private IInversionContainer _container;
        public object lockSync = new object();

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

        #region IImpersonator Members

        public bool IsImpersonated(IPrincipal principal){
            lock (lockSync){
                return impersonation.ContainsKey(principal.Identity.Name);
            }
        }


        public void Impersonate(IPrincipal principal, string userName){
            lock (lockSync){
                DeImpersonate(principal);
                var newPrincipal = new GenericPrincipal(new GenericIdentity(userName), new string[]{});
                impersonation[principal.Identity.Name] = newPrincipal;
                Active = true;
            }
        }

        public bool Active { get; protected set; }

        public void DeImpersonate(IPrincipal principal){
            lock (lockSync){
                impersonation.Remove(principal.Identity.Name);
                //force to reset
                var p = Container.get<IPrincipalSource>();
                if (p != null){
                    p.BasePrincipal = null;
                }
                if (impersonation.Count == 0){
                    Active = false;
                }
            }
        }


        public IPrincipal Resolve(IPrincipal user){
            if (impersonation.ContainsKey(user.Identity.Name)){
                return impersonation[user.Identity.Name];
            }
            return user;
        }

        #endregion
    }
}