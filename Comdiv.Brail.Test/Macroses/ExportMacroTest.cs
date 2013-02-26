using Comdiv.Brail;
using Comdiv.Extensibility.Brail;
using MvcContrib.Comdiv.Brail;
using MvcContrib.Comdiv.Extensibility.TestSupport;
using MvcContrib.Comdiv.ViewEngines.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    [TestFixture]
    [Category("Brail")]
    public class ExportMacroTest : BaseMacroTestFixture<ExportMacro>
    {
        private MyBrail brail;

        [SetUp]
        public void setup()
        {
            this.brail = new MyBrail();

        }

        [Test]
        public void can_export_named_values()
        {
            brail.Views["view1"] = @"#pragma boo
export x,1
";
            Assert.AreEqual("1", brail.Process("<%sub view1%>${x}"));
        }

        [Test]
        public void can_export_properties()
        {
            brail.Views["view1"] = @"#pragma boo
export x
defines :
    x as int
x = y + 23

";
            Assert.AreEqual("55", brail.Process("<%sub view1%>${x}", new { y = 32, x = 1 }));
        }

        [Test]
        public void can_export_values_without_rewriting_properties()
        {
            brail.Views["view1"] = @"#pragma boo
defines :
    x as int
x = y + 23
export x,2
output x
";
            Assert.AreEqual("55|2", brail.Process("<%sub view1%>|${x}", new { y = 32, x = 1 }));
        }

        [Test]
        [Ignore("it's imposible for brail to resolve")]
        public void can_work_with_locals()
        {
            brail.Views["view1"] = @"#pragma boo
defines :
    x as int
x = y + 23
export x,2
output x
";
            Assert.AreEqual("55|2", brail.Process("<%x = 1%><%sub view1%>|${x}", new { y = 32 }));
        }


    }
}