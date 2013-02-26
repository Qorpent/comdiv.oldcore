using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Comdiv.Brail.Test.Framework
{

    [TestFixture]
    public class BUGFixesInCompilation : BrailCompilerBasedTestBase
    {
        [Test]
        public void BUG_innormal_bml_structure() {
            compile(
                @"#pragma boo
bml :
    foreach i = (1,2,3,4) :
	    div ""tab_${i}"", id=""tab_${i}"", reporttab=1 :
		    table properties :
			    foreach gp=(5,6,7,8) :
				    tr property :
					    td colspan=20, style='font-size:10pt;background-color:#EEEEEE; color:black;',align=center : ""${gp}""
",
                @"current_collection = self._wrapcollection((of int: 1, 2, 3, 4))
___proceed = false
if ___proceed or (not self.isempty(current_collection)):
	_idx = 0
	if null is current_collection:
		current_collection = self._wrapcollection((of object: ,))
	for i as object in current_collection:
		self.__write(""<div class='$(self._escape(""tab_$i""))' id='$(self._escape(""tab_$i""))' reporttab='1'><table class='properties'>"")
		current_collection = self._wrapcollection((of int: 5, 6, 7, 8))
		___proceed = false
		if ___proceed or (not self.isempty(current_collection)):
			_idx = 0
			if null is current_collection:
				current_collection = self._wrapcollection((of object: ,))
			for gp as object in current_collection:
				self.__write(""<tr class='property'><td colspan='20' style='font-size:10pt;background-color:#EEEEEE; color:black;' align='center'>$gp</td></tr>"")
				_idx = (_idx + 1)
		self.__write('</table></div>')
		_idx = (_idx + 1)");
        }

    }
    [TestFixture]
    public class OutputWriteUnificationTest : BrailCompilerBasedTestBase {
        [Test]
        public void converts_outputstream_to_writes() {
            compile(@"#pragma boo
OutputStream.Write('x')
x = 1
OutputStream.Write('x')",
                    @"
self.__write('x')
x = 1
self.__write('x')
            ");
        }
        [Test]
        public void expands_nested_writes() {
            compile(@"#pragma boo
__write(__write(__write(x)))",
                    @"
self.__write(x)
            ");
        }

        [Test]
        public void joins_following_writes()
        {
            compile(@"#pragma boo
x = 1
__write('<td>')
__write(x)
__write('</td>')
x = 2
__write('<td>')
__write(x)
__write('</td>')

",
                    @"
x = 1
self.__write('<td>$x</td>')
x = 2
self.__write('<td>$x</td>')
            ");
        }
        [Test]
        public void bml_joined()
        {
            compile(@"#pragma boo
bml :
    div x :
        p : 1
",
                    @"
self.__write('<div class=\'x\'><p>1</p></div>')
            ");
        }
    }
}
