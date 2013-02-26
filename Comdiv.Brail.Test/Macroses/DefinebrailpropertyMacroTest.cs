using NUnit.Framework;

namespace Comdiv.Brail.Test{
    [TestFixture]
    [Category("Brail")]
    public class DefinebrailpropertyMacroTest : BrailMacroTestBase
    {
        [Test]
        public void simplest()
        {
            checkHtml(@"#pragma boo
definebrailproperty test1 as int = 1
output test1", "1");
        }

        [Test]
        public void vith_view_context()
        {
            checkHtml(@"#pragma boo
definebrailproperty test1 as int = 1
output test1", new { test1 = 2 }, "2");
        }

        [Test(Description = "emulates usual situation when all in input is strings, or pseudo-ints for bools")]
        public void vith_view_context_can_change_type()
        {
            checkHtml(@"#pragma boo
definebrailproperty test1 as int = 1
definebrailproperty test2 as bool = false
definebrailproperty test3 as DateTime  = DateTime.Now
output test1, test2, test3.ToString('dd.MM.yyyy')", new { test1 = "2", test2=1, test3="2009-10-11" }, "2 True 11.10.2009");
        }


        [Test]
        public void defines_block_test()
        {
            checkHtml(@"#pragma boo
defines:
    test1 as int = 1
    test2 as string = 's'
    test3 as string
    test4
    test5 = 't'
test4 = System.Object()
test3 = 'ddd'
output test1,test2,test3,test4,test5,test1.GetType(), test2.GetType(),test3.GetType(), test4.GetType(),test5.GetType()",

"1 s ddd System.Object t System.Int32 System.String System.String System.Object System.String");
        }
       
    }
}