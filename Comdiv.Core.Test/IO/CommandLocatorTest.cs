using System.Xml.Linq;
using Comdiv.IO.FileSystemScript;
using NUnit.Framework;

namespace Comdiv.Core.Test.IO {
    [TestFixture]
    public class CommandLocatorTest {
        [Test]
        public void copy_command_found_and_setted_up() {
            var x = new XElement("copy", new XAttribute("srcdir", "test"), new XAttribute("overwrite", true));
            var result = new FileSystemCommandLocator().Get(x);
            Assert.NotNull(result);
            Assert.IsInstanceOf<CopyFileSystemCommand>(result);
            var cpy = result as CopyFileSystemCommand;
            Assert.IsTrue(cpy.Overwrite);
            Assert.AreEqual("test",cpy.SrcDir);
        }
    }
}