using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Brail;
using NUnit.Framework;

namespace Classic.Brail.Test
{
    public class simpleviewsource : IViewSource {
        private string code;

        public simpleviewsource(string code) {
            this.code = code;
        }
        public Stream OpenViewStream() {
            return new MemoryStream(Encoding.UTF8.GetBytes(code));
        }

        public long LastUpdated {
            get { return 0; }
            set {  }
        }

        public long LastModified {
            get { return 0; }
        }

        public bool EnableCache {
            get { return false; }
        }
    }
    public class simpleviewsourceloader:IViewSourceLoader {
        private IDictionary<string, string> code= new Dictionary<string, string>();

        public simpleviewsourceloader(IDictionary<string,string >code) {
            this.code = code;
        }
        public bool HasSource(string sourceName) {
            return code.ContainsKey(sourceName);
        }

        public IViewSource GetViewSource(string templateName) {

            return new simpleviewsource(code[templateName.Replace(".brail","")]);
        }

        public string[] ListViews(string dirName, params string[] fileExtensionsToInclude) {
            return this.code.Keys.ToArray();
        }

        public void AddAssemblySource(AssemblySourceInfo assemblySourceInfo) {
            
        }

        public void AddPathSource(string pathSource) {
            
        }

        public string VirtualViewDir {
            get { return ""; }
            set {  }
        }

        public string ViewRootDir {
            get { return ""; }
            set { }
        }

        public bool EnableCache {
            get { return false; }
            set { }
        }

        public IList AssemblySources {
            get { return null; }
        }

        public IList PathSources {
            get { return null; }
        }

        public event FileSystemEventHandler ViewChanged;
    }

    [TestFixture]
    public class ViewEngineWorkOnClassicBrail
    {
        [Test]
        public void classictest() {
            var options = new MonoRailViewEngineOptions();
            var code = new Dictionary<string, string> {{"a", "${i}<%i=i+1%>${i}"},};
            var src = new simpleviewsourceloader(code);
            var sw = new StringWriter();
            new StandaloneBooViewEngine(src, options).Process("a", sw, new Dictionary<string, object> {{"i", 1},});
            Assert.AreEqual("12",sw.ToString());
        }
        [Test]
        public void bmltest()
        {
            var options = new MonoRailViewEngineOptions();
            var code = new Dictionary<string, string> { { "a", @"#pragma boo
bml :
    p  : ""${i}""
    i=i+1
    p : ""${i}""" }};
            var src = new simpleviewsourceloader(code);
            var sw = new StringWriter();
            new StandaloneBooViewEngine(src, options).Process("a", sw, new Dictionary<string, object> { { "i", 1 }, });
            Assert.AreEqual("<p>1</p><p>2</p>", sw.ToString());
        }
    }
}
