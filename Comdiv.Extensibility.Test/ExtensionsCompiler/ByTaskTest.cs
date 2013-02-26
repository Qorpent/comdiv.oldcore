using System.IO;
using Comdiv.Application;
using Comdiv.Extensibility.ExtensionsCompiler;
using Comdiv.IO;
using NUnit.Framework;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    [Category("INTEGRATION")]
    [Category("LONGEXEC")]
    public class ByTaskTest {
        [SetUp]
        public void setup() {
            FilePathResolverExtensions.Write(myapp.files, "~/extensions/a.boo",
                                             @"class ByCompilerTest:
    pass
registry['x'] = ByCompilerTest");

            try {
            	Directory.CreateDirectory("tmp");
                File.Delete("tmp\\ByTaskTest.dll");
            }
            catch {
            }
        }

        [Test]
        public void can_generate_assembly() {
            Assert.False(File.Exists("tmp\\ByTaskTest.dll"));
            var task = new ApplicationExtensionsLoadTask(dllname:"tmp\\ByTaskTest",web:false);
            task.Start();
            var assembly = task.Finish();
            Assert.True(File.Exists("tmp\\ByTaskTest.dll"));
            var r = new ExtensionsLoader().GetRegistry(assembly);
            Assert.True(r.ContainsKey("x"));
        }
    }
}