using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Comdiv.Security;
using NUnit.Framework;

namespace Comdiv.Core.Test.Security {
    [TestFixture]
    public class DefaultRoleResolverTest{
        private RoleResolver r;
        private IPrincipal p;

        public DefaultRoleResolverTest(){
           r = new RoleResolver();
           p = Thread.CurrentPrincipal;
        }
        [Test]
        public void well_admin_behaviour_on_isinrole(){
           
            Assert.False(r.IsInRole(p,"r1"));
            Assert.False(r.IsInRole(p,"r2"));
            r.Assign(p, "r1");
            Assert.True(r.IsInRole(p,"r1"));
            Assert.False(r.IsInRole(p,"r2"));
            r.Assign(p, "ADMIN");
            Assert.True(r.IsInRole(p,"r1"));
            Assert.True(r.IsInRole(p,"r2"));
            Assert.True(r.IsInRole(p,"r1",false));
            Assert.False(r.IsInRole(p,"r2",false));
            
        }
    }
}
