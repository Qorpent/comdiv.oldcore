using System;
using System.IO;
using System.Threading;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using Comdiv.IO;
using NUnit.Framework;

namespace Comdiv.Brail.Test.Framework
{
    [TestFixture]
    public class ViewSourceResolverTest
    {
        private BrailSourceResolver loader;

        [SetUp]
        public void setup() {
            var dir = "ViewSourceResolverTest".prepareTemporaryDirectory(true);
            this.loader = new BrailSourceResolver {FileSystem = new DefaultFilePathResolver(dir)};

        }
        [TearDown]
        public void teardown() {
            loader.Dispose();
        }

        public void setext(string  extname, string viewname, string code) {
            set("extensions/"+extname+"/"+viewname,code);
        }
        public void setroot(string area, string viewname, string code)
        {
            set("views/" + area + "/" + viewname, code);
        }
        public void setusr(string area, string viewname, string code)
        {
            set("usr/views/" + area + "/" + viewname, code);
        }
        public void setsys(string area, string viewname, string code)
        {
            set("sys/views/" + area + "/" + viewname, code);
        }
        public void set(string name, string code) {
            loader.FileSystem.Write("~/"+name+".brail",code);
        }

        [Test]
        public void can_get_code() {
            setsys("test","x","test1");
            Assert.AreEqual("test1",loader.GetCode("/test/x"));
        }

        [Test]
        public void can_give_full_info() {
            setsys("test", "x", "test1");
            setusr("test", "x2", "test2");
            var info = loader.GetFullInfo("/test/x");
            Assert.True(Path.IsPathRooted(info.FileName));
            Assert.AreEqual("/test/x",info.Key);
            Assert.True(File.Exists(info.FileName));
            Assert.AreEqual(File.GetLastWriteTime(info.FileName),info.LastModified);
            Assert.AreEqual(2,info.Level);
            Assert.AreEqual(File.ReadAllText(info.FileName),info.GetContent());
        }

        [Test]
        public void normal_level_resolution()
        {
            setsys("test", "x", "test1");
            setusr("test", "x", "test2");
            Assert.AreEqual("test2", loader.GetCode("/test/x"));
        }
        [Test]
        public void can_check_valid()
        {
            setsys("test", "x", "test1");
            Assert.True(loader.IsValid("/test/x",DateTime.Now));
            Assert.False(loader.IsValid("/test/x", DateTime.Now.AddMinutes(-1)));
        }
        [Test]
        public void supports_inmemory_views() {
            loader.SetStatic("/test/x","test1");
            Assert.AreEqual("test1", loader.GetCode("/test/x"));
        }
        [Test]
        public void supports_inmemory_validation()
        {
            loader.SetStatic("/test/x", "test1");
            Assert.True(loader.IsValid("/test/x", DateTime.Now));
            Assert.False(loader.IsValid("/test/x", DateTime.Now.AddMinutes(-1)));
        }
        [Test]
        public void inmemory_overrides_files()
        {
            setsys("test", "x", "test2");
            Assert.AreEqual("test2", loader.GetCode("/test/x"));
            loader.SetStatic("/test/x", "test1");
            Assert.AreEqual("test1", loader.GetCode("/test/x"));
            
        }
        [Test]
        public void inmemory_preserves()
        {
            loader.SetStatic("/test/x", "test1");
            setsys("test", "x", "test2");
            loader.Reload();
            Assert.AreEqual("test1", loader.GetCode("/test/x"));

        }

        [Test]
        public void changes_are_tracked() {
            setsys("test","x","test2");
            var testfile = loader.GetFullInfo("/test/x");
            ViewCodeSource file = null;
            bool affected  = false;
            loader.Changed += (l, f, a) =>
                                  {
                                      file = f;
                                      affected = a;
                                  };
            Thread.Sleep(200);
            setsys("test", "x", "test3");
            Thread.Sleep(200);
            Assert.NotNull(file);
            Assert.AreEqual(testfile.FileName.ToLower(),file.FileName.ToLower());
            Assert.True(affected);
        }

        [Test]
        public void changes_are_tracked_and_affctation_meet_expectations()
        {
            setsys("test", "x", "test2");
            setusr("test", "x", "test3");
            var testfile = loader.GetFullInfo("/test/x");
            ViewCodeSource file = null;
            bool affected = false;
            loader.Changed += (l, f, a) =>
            {
                file = f;
                affected = a;
            };
            Thread.Sleep(200);
            setsys("test", "x", "test3");
            Thread.Sleep(200);
            Assert.NotNull(file);
            Assert.AreNotEqual(testfile.FileName, file.FileName);
            Assert.False(affected);
        }
    }
}
