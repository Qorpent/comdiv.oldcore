using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.IO.FileSystemScript;
using Comdiv.MAS;
using NUnit.Framework;

namespace Comdiv.Core.Test.IO
{
    [TestFixture]
    public class CopyFileSystemCommandTest
    {
        private CopyFileSystemCommand cmd;
        private string root;
        void setsrc(string name, string txt = null) {
            txt = txt ?? name;
            cmd.Filesystem.Write("~/src/"+name,txt);
        }
        void checktrg(string name, string txt =null) {
            txt = txt ?? name;
            name = "~/trg/" + name;
            Assert.True(cmd.Filesystem.Exists(name));
            Assert.AreEqual(txt,cmd.Filesystem.Read(name));
        }
        [SetUp]
        public void setup() {
            this.root = "CopyFileSystemCommandTest".prepareTemporaryDirectory(true);
            this.cmd = new CopyFileSystemCommand();
            cmd.Filesystem = new DefaultFilePathResolver(root);
            cmd.Log = new LogHostBase {Writer = Console.Out};
            cmd.SrcDir = "~/src/";
            cmd.TrgDir = "~/trg/";
            cmd.Mask = "*.txt";
        }

        [Test]
        public void simple_one_file() {
            setsrc("1.txt");
            cmd.Execute();
            checktrg("1.txt");
        }

        [Test]
        public void don_t_overwrite()
        {
            setsrc("1.txt");
            cmd.Execute();
            cmd.Filesystem.Write("~/trg/1.txt","x");
            cmd.Execute();
            checktrg("1.txt","x");
        }
        [Test]
        public void can_overwrite()
        {
            setsrc("1.txt");
            cmd.Execute();
            cmd.Filesystem.Write("~/trg/1.txt", "x");
            cmd.Overwrite = true;
            cmd.Execute();
            checktrg("1.txt", "1.txt");
        }

        [Test]
        public void can_collect()
        {
            setsrc("a/1.txt");
            setsrc("b/2.txt");
            cmd.Execute();
            checktrg("1.txt","a/1.txt");
            checktrg("2.txt", "b/2.txt");
        }


        [Test]
        public void can_keep_paths() {
            cmd.KeepPaths = true;
            setsrc("a/1.txt");
            setsrc("b/2.txt");
            cmd.Execute();
            checktrg("a/1.txt", "a/1.txt");
            checktrg("b/2.txt", "b/2.txt");
        }

    }
}
