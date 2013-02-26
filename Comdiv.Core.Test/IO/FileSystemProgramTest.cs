using System;
using Comdiv.Booxml;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.IO.FileSystemScript;
using Comdiv.MAS;
using NUnit.Framework;

namespace Comdiv.Core.Test.IO {
    [TestFixture]
    public class FileSystemProgramTest {
        private FileSystemProgram prog;
        private string root;
        void setsrc(string name, string txt = null)
        {
            txt = txt ?? name;
            prog.Filesystem.Write("~/src/" + name, txt);
        }
        void checktrg(string name, string txt = null)
        {
            txt = txt ?? name;
            name = "~/trg/" + name;
            Assert.True(prog.Filesystem.Exists(name));
            Assert.AreEqual(txt, prog.Filesystem.Read(name));
        }
        [SetUp]
        public void setup()
        {
            this.root = "CopyFileSystemCommandTest".prepareTemporaryDirectory(true);
            this.prog = new FileSystemProgram();
            prog.Filesystem = new DefaultFilePathResolver(root);
            prog.Log = new LogHostBase { Writer = Console.Out };
            var progtxt = @"
setup srcdir='~/src', trgdir='~/trg'
setup mask='1.txt'
copy srcdir='${srcdir}', trgdir='${trgdir}', mask='${mask}'
setup mask='2.txt'
copy srcdir='${srcdir}', trgdir='${trgdir}', mask='${mask}'";
            prog.Load(new BooxmlParser().Parse(progtxt));
        }

        [Test]
        public void can_collect()
        {
            setsrc("a/1.txt");
            setsrc("b/2.txt");
            setsrc("b/3.txt");
            prog.Execute();
            checktrg("1.txt", "a/1.txt");
            checktrg("2.txt", "b/2.txt");
            Assert.False(prog.Filesystem.Exists("~/trg/3.txt"));
        }
    }
}