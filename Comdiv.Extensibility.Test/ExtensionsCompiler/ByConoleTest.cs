using System;
using System.IO;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensibility.ExtensionsCompiler;
using NUnit.Framework;
using FilePathResolverExtensions = Comdiv.IO.FilePathResolverExtensions;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    public class ByConoleTest  {
        [SetUp]
        public  void setup() {
            FilePathResolverExtensions.Write(myapp.files, "~/extensions/a.boo", @"class ByConoleTest:
    pass
registry['x'] = ByConoleTest");
             try {
             	Directory.CreateDirectory("tmp");
                File.Delete("tmp\\ByConoleTest.dll");
            }
            catch {
            }
        }

        [Test]
        public void can_generate_assembly() {
            var c = new ExtensionsCompilerConsoleProgram();
            var result = c.Execute(new ExtensionsCompilerConsoleProgramArgs(){DllName="tmp\\ByConoleTest",Root = "",Web = false});
            Assert.NotNull(result);
            Assert.NotNull(result.GetType("ByConoleTest"));
            Assert.NotNull(result.GetType("L0_extensions_a"));
            Assert.True(result.GetType("L0_extensions_a").GetInterfaces().Contains(typeof(IRegistryLoader)));
        }
       
    }
}