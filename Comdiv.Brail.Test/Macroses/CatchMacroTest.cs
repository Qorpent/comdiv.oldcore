using NUnit.Framework;

namespace Comdiv.Brail.Test{
    [TestFixture]
    [Category("Brail")]
    public class CatchMacroTest : BrailMacroTestBase
    {
        [Test]
        public void simplest(){
            checkHtml(@"#pragma boo
catch:
    output 1
output 'x',_out
output 'x',_out
", "x 1x 1");
        }

        [Test]
        public void varname_specified()
        {
            checkHtml(@"#pragma boo
catch myvar:
    output 1
output 'x',myvar
output 'x',myvar
", "x 1x 1");
        }
    }
}