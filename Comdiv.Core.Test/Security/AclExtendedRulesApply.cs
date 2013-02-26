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
using System.Text;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.Model;
using Comdiv.Security;
using Comdiv.Security.Acl;
using NUnit.Framework;

namespace Comdiv.Security.Tests
{
    [TestFixture]
    public class AclExtendedRulesApplyTest
    {
        [SetUp]
        public void setup(){
            ioc.finish();
            ioc.Container.setupSecurity();
        }
        [Test]
        public void inactive_rules_must_not_by_apply_able(){
            acl.repository.deny("/test1", "test1");
            myapp.SetCurrentUser("test1");
            Assert.False(acl.get("/test1",true));
            acl.repository.deny("/test2", "test2",x=>x.Active=false);
            myapp.SetCurrentUser("test2");
            Assert.True(acl.get("/test2", true));
        }
        [Test]
        public void insufient_date_rules_must_not_by_apply_able()
        {
            acl.repository.deny("/test1", "test1");
            myapp.SetCurrentUser("test1");
            Assert.False(acl.get("/test1", true));
            acl.repository.deny("/test2", "test2", x => { x.StartDate = DateTime.Today.AddDays(-1);
                                                       x.EndDate = DateTime.Today.AddDays(1); });
            myapp.SetCurrentUser("test2");
            Assert.False(acl.get("/test2", true));
            acl.repository.deny("/test3", "test3", x => x.StartDate=DateTime.Today.AddDays(1));
            myapp.SetCurrentUser("test3");
            Assert.True(acl.get("/test3", true));
            acl.repository.deny("/test4", "test4", x => x.EndDate = DateTime.Today.AddDays(-1));
            myapp.SetCurrentUser("test4");
            Assert.True(acl.get("/test4", true));
        }
    }
}
