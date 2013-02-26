using System;
using System.Collections.Generic;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.Application;
using Comdiv.Extensibility.ExtensionsCompiler;
using Comdiv.Extensions;
using Comdiv.IO;
using NUnit.Framework;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    public class WellOrderOfApply  {
        [Test]
        public void need_well_formed_registry() {
            myapp.files.Write("~/sys/extensions/zzz.boo", @"
class A :
    pass
registry['a'] = A
");
            myapp.files.Write("~/usr/extensions/aaa.boo", @"
class Z :
    pass
registry['a'] = Z
");
            var efs = new ExtensionsFileSystemProvider();
            var ex = new ExtensionsCompilerExecutor{Pipeline = new CompileToMemory()};
            var a = ex.Compile(efs);
            var result = new ExtensionsLoader().GetRegistry(a);
            Assert.AreEqual("Z",((Type)result["a"]).Name);

        }
    }

    [TestFixture]
    public class CodeIsCompileableTest : ExtensionsPreprocessorTestBase {
        protected override Module postProcess() {
            return srcmodule;
        }

        protected override void prepareCompiler(CompilerParameters parameters) {
            base.prepareCompiler(parameters);
            parameters.References.Add(typeof(IRegistryLoader).Assembly);
            parameters.References.Add(typeof(IDictionary<string,string>).Assembly);
        }

        protected override CompilerPipeline getPipeline() {
            return ExtensionsPreprocessorCompilerStep.Extend(new CompileToMemory());
        }

        [Test]
        public void assembly_generated() {
            Assert.NotNull(this.cu.GeneratedAssembly);
        }

        [Test]
        public void class_test_exists() {
            Assert.NotNull(this.cu.GeneratedAssembly.GetType("L0__test"));
        }

        [Test]
        public void class_test_support_needed_interface() {
            var ex = cu.GeneratedAssembly.GetType("L0__test").create();
            Assert.IsInstanceOf<IRegistryLoader>(ex);
        }

        [Test]
        public void class_test_method_works() {
            var ex = cu.GeneratedAssembly.GetType("L0__test").create() as IRegistryLoader;
            var registry = new Dictionary<string, object>();
            ex.Load(registry);
            Assert.AreEqual("X", (registry["x"] as Type).Name);

        }
    }
}