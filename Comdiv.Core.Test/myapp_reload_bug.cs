using System;
using Comdiv.Application;
using NUnit.Framework;

namespace Comdiv.Core.Test {
    [TestFixture(Description = "myapp reload test - bug was with exception handling")]
    public class myapp_reload_bug {
        [Test]
        public void on_reload_need_return_valid_group_exception() {
            myapp.clearReloads();
            myapp.OnReload += (s, a) => { throw new Exception("message 2"); };
            Exception e = myapp.reload();
            Assert.NotNull(e);
            StringAssert.Contains("on_reload_need_return_valid_group_exception",e.ToString());
            StringAssert.Contains("message 2", e.ToString());
          
        }
    }
}