using MvcContrib.Comdiv.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test
{
    [TestFixture]
    public class SubviewPropertyExportingTest:BrailMacroTestBase
    {
        private MyBrail brail;

        [SetUp]
        public void setup(){
            this.brail = new MyBrail();

        }

        [Test]
        public void can_export_named_values(){
            brail.Views["view1"] = @"#pragma boo
__Export('x',1)
";
            Assert.AreEqual("1", brail.Process("<%sub view1%>${x}"));
        }

        [Test]
        public void can_export_properties()
        {
            brail.Views["view1"] = @"#pragma boo
defines :
    x as int
x = y + 23
__Export('x')
";
            Assert.AreEqual("55", brail.Process("<%sub view1%>${x}", new { y = 32, x=1 }));
        }

        [Test]
        public void can_export_values_without_rewriting_properties()
        {
            brail.Views["view1"] = @"#pragma boo
defines :
    x as int
x = y + 23
__Export('x',2)
output x
";
            Assert.AreEqual("55|2", brail.Process("<%sub view1%>|${x}", new { y = 32, x = 1 }));
        }
       
    }
}
