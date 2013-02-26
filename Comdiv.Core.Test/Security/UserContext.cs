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
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Comdiv.Application;
using Comdiv.Inversion;
using Qorpent.Security;

namespace Comdiv.Security{
    /// <summary>
    /// Setups and restores thread principal context for
    /// IPrincipalSource, intended to use in "using" clause
    /// </summary>
    public class UserContext : IDisposable{
        public UserContext(string name,string role,IPrincipalSource source):this(new GenericPrincipal(new GenericIdentity(name),new[]{role}),source){
            
        }
        
        public UserContext(IPrincipal user) : this(user, myapp.ioc.get<IPrincipalSource>()) {}

       

        public UserContext(IPrincipal user,IPrincipalSource principalSource){
            PrincipalSource = principalSource;
            enter(user);
        }

        private void enter(IPrincipal user) {
            beforeEnter(user);
            if (null != PrincipalSource){
                Restore = PrincipalSource.CurrentUser;
                PrincipalSource.BasePrincipal = user;
                //Thread.CurrentPrincipal = user;
                
            }else{
                Restore = Thread.CurrentPrincipal;
                Thread.CurrentPrincipal = user;
            }
            
        }

        protected virtual void beforeEnter(IPrincipal user){
            
        }

        private IPrincipalSource PrincipalSource { get; set; }

        public IPrincipal Restore { get; set; }

        #region IDisposable Members

        public void Dispose(){
            exit();
        }

        private void exit() {
            if (null != PrincipalSource){
                PrincipalSource.BasePrincipal = Restore;
            }else{
                Thread.CurrentPrincipal = Restore;
            }
            
            afterExit();
        }

        protected virtual void afterExit(){
            
        }

        #endregion
    }
}