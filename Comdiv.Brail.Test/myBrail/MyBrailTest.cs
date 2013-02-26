using MvcContrib.Comdiv.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test
{
    [TestFixture]
    public class MyBrailTest : BrailMacroTestBase
    {
        private MyBrail brail;

        [SetUp]
        public void setup(){
            this.brail = new MyBrail();
        }
        [Test]
        public void can_process_usual_brail(){
            Assert.AreEqual("1", brail.Process("<%x=1%>${x}"));
        }

        [Test]
        public void can_process_boo_syntaxed_brail(){
            Assert.AreEqual("1", brail.Process("#pragma boo\r\nx=1\r\noutput x"));
        }

        [Test]
        public void can_bind_parameters_from_anonymous(){
            Assert.AreEqual("32", brail.Process("#pragma boo\r\noutput x", new { x = 32 }));
        }

        [Test]
        public void provides_subview_support()
        {
            brail.Views["view1"] = "#pragma boo\r\noutput x";
            Assert.AreEqual("before32after", brail.Process("before<%sub view1%>after", new { x = 32 }));
        }

        [Test]
        public void provides_layout_support()
        {
            brail.Views["layout1"] = "before${ChildOutput}after";
            Assert.AreEqual("before32after", brail.Process("${x}", new { x = 32 },"layout1"));
        }

    
    }
}
