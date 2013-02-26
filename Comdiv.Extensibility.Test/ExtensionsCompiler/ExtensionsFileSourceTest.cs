using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Comdiv.Application;
using Comdiv.Extensibility.ExtensionsCompiler;
using Comdiv.Extensions;
using Comdiv.IO;
using NUnit.Framework;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    public class ExtensionsFileSourceTest {
        private ExtensionsFileSystemProvider fs;

        [SetUp]
        public void setup() {
            //first cleanup files
            cleanup();
            this.fs = new ExtensionsFileSystemProvider();
            myapp.files.Write("~/extensions/a.boo");
            myapp.files.Write("~/extensions/b.boo");
            myapp.files.Write("~/extensions/c.boo");
            myapp.files.Write("~/extensions/d.boo");
            myapp.files.Write("~/sys/extensions/a.boo");
            myapp.files.Write("~/sys/extensions/b.boo");
            myapp.files.Write("~/sys/extensions/c.boo");
            myapp.files.Write("~/usr/extensions/a.boo");
            myapp.files.Write("~/usr/extensions/b.boo");
            myapp.files.Delete(fs.GetHashPath());
            
            myapp.files.Delete(fs.GetLibraryPath());
        }
        [TearDown]
        public void cleanup() {
            var filestodelete = myapp.files.ResolveAll((string) "extensions", (string) "*.boo", (bool) true, (Action<string>) null);
            foreach (var file in filestodelete) {
                File.Delete(file);
            }
            myapp.files.Delete("~/tmp/ExtensionFilesSource_hash");
            myapp.files.Delete("~/tmp/extensions.dll");
         
        }

        [Test]
        public void file_count_well() {
            Assert.AreEqual(4, fs.GetFileNames().Count());
        }

        [Test]
        public void file_levels_well() {
            Assert.AreEqual(1, fs.GetFileNames().Where(x=>x.Contains("/sys/")).Count());
            Assert.AreEqual(2, fs.GetFileNames().Where(x=>x.Contains("/usr/")).Count());
        }

        [Test]
        public void initial_version_is_close_to_now() {
            Assert.Greater(fs.GetSourceVersion(),DateTime.Now.AddSeconds(-2));
           
        }

        [Test]
        public void repeated_last_versions_are_equal() {
            var lv = fs.GetSourceVersion();
            var newfs = new ExtensionsFileSystemProvider();
            Thread.Sleep(10);
            //check repeated creation
            Assert.AreEqual(lv, newfs.GetSourceVersion());
        }

         [Test]
        public void file_changes_affect_version() {
            var lv = fs.GetSourceVersion();
            var newfs = new ExtensionsFileSystemProvider();
            myapp.files.Write("~/sys/extensions/c.boo","1");
            Assert.Less(lv, newfs.GetSourceVersion());
        }

           [Test]
        public void file_creations_affect_version() {
            var lv = fs.GetSourceVersion();
            var newfs = new ExtensionsFileSystemProvider();
            myapp.files.Write("~/sys/extensions/e.boo","1");
            Assert.Less(lv, newfs.GetSourceVersion());
        }

        [Test]
        public void file_deletion_affected_lastversion() {
            var lv = fs.GetSourceVersion();
            var newfs = new ExtensionsFileSystemProvider();
            File.Delete( myapp.files.Resolve("~/usr/extensions/a.boo"));
            Assert.Less(lv, newfs.GetSourceVersion());
        }

     

         [Test]
        public void non_used_file_changes_not_affect_version() {
            var lv = fs.GetSourceVersion();
            var newfs = new ExtensionsFileSystemProvider();
            myapp.files.Write("~/sys/extensions/a.boo","1");
            Assert.AreEqual(lv, newfs.GetSourceVersion());
        }

         [Test]
        public void extensions_dll_version_is_begin() {
            Assert.AreEqual(DateExtensions.Begin,fs.GetLibraryVersion());
        }

        [Test]
        public void can_define_assembly_list() {
            Assert.Contains(typeof(ExtensionsFileSourceTest).Assembly.FullName,fs.GetReferencedAssemblies().Select(x=>x.FullName).ToArray());
        }
    }
}
