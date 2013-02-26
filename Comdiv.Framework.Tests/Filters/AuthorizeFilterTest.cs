using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Security;
using NUnit.Framework;

namespace Comdiv.Framework.Tests.Filters
{
    [TestFixture]
    public class AuthorizeFilterTest
    {
        private AuthorizeFilter filter;

        public class empty:IController {
            public void Dispose() {
                throw new NotImplementedException();
            }

            public void Process(IEngineContext engineContext, IControllerContext context) {
                throw new NotImplementedException();
            }

            public void PreSendView(object view) {
                throw new NotImplementedException();
            }

            public void PostSendView(object view) {
                throw new NotImplementedException();
            }

            public event ControllerHandler BeforeAction;
            public event ControllerHandler AfterAction;
        }

        [Role("r1,r2")]
        public class testController:IController {
            public void Dispose() {
                throw new NotImplementedException();
            }

            public void Process(IEngineContext engineContext, IControllerContext context) {
                throw new NotImplementedException();
            }

            public void PreSendView(object view) {
                throw new NotImplementedException();
            }

            public void PostSendView(object view) {
                throw new NotImplementedException();
            }

            public event ControllerHandler BeforeAction;
            public event ControllerHandler AfterAction;

            public void test() {
                
            }
            [Role("r3")]
            public void test2()
            {

            }

            [Public]
            public void test3()
            {

            }
        }

        [SetUp]
        public void setup() {
            this.filter = new AuthorizeFilter();
            ((RoleResolver)myapp.roles).Reload();
        }
        private bool exec(bool emptycontroller, string action, string user, params string[] roles)
        {
            var c = (emptycontroller ? new empty() : (IController)new testController());
            var cn = emptycontroller ? "empty" : "test";
            myapp.principals.BasePrincipal = user.toPrincipal(roles);
            var context = (IControllerContext)new ControllerContext(cn, action, null);
            return filter.AuthorizeContext(context, c);
        }
        private bool execf(bool emptycontroller,string action, string user, params string[] roles) {
            var c = (emptycontroller ? new empty() : (IController)new testController());
            var cn = emptycontroller ? "empty" : "test";
            myapp.principals.BasePrincipal = user.toPrincipal(roles);
            var context = (IControllerContext) new ControllerContext(cn,action,null);
            return filter.Perform(ExecuteWhen.BeforeAction, null, c, context);
        }

        [Test]
        public void returns_true_by_default() {
            Assert.True(exec(true,"any","any"));
        }

        [Test]
        public void returns_true_for_admin()
        {
            Assert.True(exec(false, "test", "adm", "ADMIN"));
        }

        [Test]
        public void returns_true_for_public_actions()
        {
            Assert.True(exec(false, "test3", "nmu"));
        }

        [Test]
        public void returns_false_for_non_matched_users()
        {
            Assert.False(exec(false, "test", "nmu","r4"));
        }

        [Test]
        public void returns_true_for_special_action_role()
        {
            Assert.True(exec(false, "test", "sar", "ALLOW_TEST_TEST"));
        }

        [Test]
        public void can_deny_access_to_action()
        {
            Assert.False(exec(false, "test", "tdaa", "r1","DENY_TEST_TEST"));
        }


        [Test]
        public void admin_override_deny()
        {
            Assert.True(exec(false, "test", "aod", "ADMIN", "DENY_TEST_TEST"));
        }

        [Test]
        public void action_deny_override_controller_allow()
        {
            Assert.False(exec(false, "test", "adoca", "r1", "ALLOW_TEST","DENY_TEST_TEST"));
        }

        [Test]
        public void action_allow_override_controller_deny()
        {
            Assert.True(exec(false, "test", "adoca", "r1", "DENY_TEST", "ALLOW_TEST_TEST"));
        }

        [Test]
        public void returns_true_for_special_controller_role()
        {
            Assert.True(exec(false, "test", "scr", "ALLOW_TEST"));
            Assert.True(exec(false, "test2", "scr", "ALLOW_TEST"));
        }

        [Test]
        public void can_deny_access_to_controller()
        {
            Assert.False(exec(false, "test", "tdac", "r1", "DENY_TEST"));
        }

        [Test]
        public void returns_true_for_assigned_roles()
        {
            Assert.True(exec(false, "test", "sasr", "r1"));
            Assert.True(exec(false, "test", "sasr2", "r2"));
            Assert.True(exec(false, "test2", "sasr3", "r3"));
        }
    }
}
