using System;
using Comdiv.Common;
using NUnit.Framework;

namespace Comdiv.Core.Test {
    [TestFixture(Description = "проверяет ренедеринг ToString в GroupException")]
    public class GroupExceptionTest {
        [Test]
        public void well_formed_group_exception_to_string() {
            var e = new GroupException("message 1", new[]{new Exception("message 2")});
            Assert.AreEqual(
                @"message 1
-----------------------------
Exception: message 2 
System.Exception: message 2
-----------------------------
",
                e.ToString());
        }
    }
}