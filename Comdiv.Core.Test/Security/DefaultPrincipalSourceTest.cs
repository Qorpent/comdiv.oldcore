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
using NUnit.Framework;

namespace Comdiv.Security.Tests{
    [TestFixture]
    public class DefaultPrincipalSourceTest{
        #region Setup/Teardown

        [SetUp]
        public void setup(){
            source = new PrincipalSource();
            testuser = new GenericPrincipal(new GenericIdentity("test"), new string[]{});
            source.BasePrincipal = null;
        }

        #endregion

        private PrincipalSource source;
        private GenericPrincipal testuser;

        public class MyImp : IImpersonator{
            public GenericPrincipal impuser = new GenericPrincipal(new GenericIdentity("test2"), new string[]{});

            public bool Active{
                get { return true; }
            }

            public void Impersonate(IPrincipal principal, string userName){
                throw new NotImplementedException();
            }

            public void DeImpersonate(IPrincipal principal){
                throw new NotImplementedException();
            }

            public IPrincipal Resolve(IPrincipal user){
                if (user.Identity.Name == "test"){
                    return impuser;
                }
                return user;
            }

            public bool IsImpersonated(IPrincipal principal){
                throw new NotImplementedException();
            }
        }


        [Test]
        public void by_default_principal_source_return_current_user(){
            Assert.NotNull(source.BasePrincipal);
            Assert.NotNull(source.Current);
            Assert.AreEqual(UserName.For(source.BasePrincipal).Name, UserName.For(source.Current).Name);

        }

        [Test]
        public void can_change_current_user(){
            source.BasePrincipal = testuser;
            Assert.AreSame(testuser, source.Current);
        }

        [Test]
        public void can_use_impersonation(){
            var myimp = new MyImp();
            source.Impersonator = myimp;
            Assert.AreEqual(UserName.For(Environment.UserName).Name, UserName.For(source.Current).Name);
            source.BasePrincipal = testuser;
            Assert.AreEqual(UserName.For(myimp.impuser).Name,UserName.For( source.Current).Name);
            Assert.AreNotEqual(UserName.For(source.BasePrincipal).Name,UserName.For( source.Current).Name);
        }
    }
}