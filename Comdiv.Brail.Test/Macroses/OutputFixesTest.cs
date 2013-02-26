using System;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    [TestFixture]
    [Category("Brail")]
    public class OutputFixesMacroTest : BrailMacroTestBase
    {
        [Test]
        public void can_render_interpolation()
        {
            checkHtml(
                @"#pragma boo
output ""${1}""",

                @"1");
        }

        [Test]
        public void can_render_3_q_strings_with_interpolations()
        {
            checkHtml(
                @"#pragma boo
output """"""${1}""""""",

                @"1");
        }

        [Test]
        public void cover_compilation_error_on_no_args(){
            TestDelegate err = () => checkHtml(
                @"#pragma boo
output
",

                @"1");
            var ex = Assert.Throws<Exception>(err);
        }
    }
}