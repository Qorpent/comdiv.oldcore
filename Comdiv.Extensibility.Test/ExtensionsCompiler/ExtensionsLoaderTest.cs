using System;
using Comdiv.Extensibility.ExtensionsCompiler;
using NUnit.Framework;


namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    public class ExtensionsLoaderTest:CodeIsCompileableTest {
        [Test]
        public void loads_registry() {
            var registry = new ExtensionsLoader().GetRegistry(cu.GeneratedAssembly);
            Assert.AreEqual(1,registry.Count);
            Assert.IsInstanceOf<Type>(registry["x"]);
            Assert.AreEqual("X",((Type)registry["x"]).Name);
        }
    }
}