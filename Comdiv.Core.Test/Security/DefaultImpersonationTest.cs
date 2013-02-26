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
using System.Text;
using System.Threading;
using Comdiv.Security;
using NUnit.Framework;

namespace Comdiv.Security.Tests
{
    [TestFixture]
    public class DefaultImpersonationTest
    {
        private Impersonator imp;
        private GenericPrincipal testuser;


        [SetUp]
        public void setup(){
            this.imp = new Impersonator();
            this.testuser = new GenericPrincipal(new GenericIdentity("test"), new string[] { });
            
            
        }
        
        [Test]
        public void main_lyfe_cycle_of_impersonation(){
            Assert.AreSame(testuser, imp.Resolve(testuser));
            Assert.False(imp.IsImpersonated(testuser));
            imp.Impersonate(testuser,"test3");
            Assert.True(imp.IsImpersonated(testuser));
            Assert.AreEqual("test3", imp.Resolve(testuser).Identity.Name);
            imp.DeImpersonate(testuser);
            Assert.AreSame(testuser, imp.Resolve(testuser));
            Assert.False(imp.IsImpersonated(testuser));
            
        }
    }
}
