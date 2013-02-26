using System;
using System.Collections.Generic;
using System.IO;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.Application;
using Comdiv.Extensibility.ExtensionsCompiler;
using Comdiv.Extensions;
using Comdiv.IO;
using NUnit.Framework;
using System.Linq;
namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    public class ByCompilerTest  {
        [SetUp]
        public  void setup() {
            myapp.files.Write("~/extensions/a.boo", @"class ByCompilerTest:
    pass
registry['x'] = ByCompilerTest");
			try {
				Directory.CreateDirectory("tmp");
				File.Delete("tmp\\ByCompilerTest.dll");
			}catch {
				
			}
        }

        [Test]
        public void can_generate_assembly() {
            var c = new ExtensionsCompilerExecutor();
			var result = c.Compile(new ExtensionsFileSystemProvider() { DllName = "tmp\\ByCompilerTest.dll", Web=false});
            Assert.NotNull(result);
            Assert.NotNull(result.GetType("ByCompilerTest"));
            Assert.NotNull(result.GetType("L0_extensions_a"));
            Assert.True(result.GetType("L0_extensions_a").GetInterfaces().Contains(typeof(IRegistryLoader)));
        }
       
    }
}