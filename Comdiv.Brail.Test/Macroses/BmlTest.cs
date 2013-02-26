using System;
using System.IO;
using Boo.Lang.Compiler.Pipelines;
using NUnit.Framework;

namespace Comdiv.Brail.Test
{
    [TestFixture]
    public class BmlTest : BrailMacroTestBase
    {

        [Test]
        public void direct_attributes_overrides_defaults()
        {
            checkHtml(@"
#pragma boo
bml:
    style type=nocss : """"""#id {weight:100%;}""""""
",
 @"<style type='nocss'>#id {weight:100%;}</style>"
                );
        }

        [Test]
        public void normal_expansion()
        {
            checkHtml(@"
#pragma boo
x = 1
bml:
    div cls: x
",
 @"<div class='cls'>1</div>"
                );
        }

        [Test]
        public void start_tag_only()
        {
            checkHtml(@"
#pragma boo
x = 1
bml:
    div_ 'class'='cls'
",
 @"<div class='cls'>"
                );
        }

        [Test]
        public void end_tag_only()
        {
            checkHtml(@"
#pragma boo
x = 1
bml:
    _div
",
 @"</div>"
                );
        }


        [Test]
        public void indexers_applyed_as_writeout()
        {
            checkHtml(@"
#pragma boo
bml:
    x = (1,2)
    a href=x[0] : x[1]
",
 @"<a href='1'>2</a>"
                );
        }

        [Test]
        public void normal_expansion_with_var_and_string_propagation()
        {
            checkHtml(@"
#pragma boo
x = 1
bml:
    div cls:
        @x
        output x+2
        @x.ToString()
        ""str""
",
 @"<div class='cls'>131str</div>"
                );
        }

        [Test]
        public void normal_expansion_with_strings_after_if_and_outnamed_defs_WAS_BUG()
        {
            checkHtml(@"
#pragma boo
def outsome():
    output 'some'
def some() as string:
    return  's'

x=1
bml:
    div cls:
        if true :
            1
        
        ""str""
        """"""interpolation${x}""""""
        outsome()
        some()
        @some()
        @x
        try :
            1
            b = 2
        except:
            @x
        for j in (1,2,3):
            @j
        
",
 @"<div class='cls'>1strinterpolation1somes11123</div>"
                );
        }
        

        


        [Test]
        public void style_got_thier_type()
        {
            checkHtml(@"
#pragma boo
bml:
    style: """"""#id {weight:100%;}""""""
",
 @"<style type='text/css'>#id {weight:100%;}</style>"
                );
        }

        [Test]
        public void basic_test_start_for_developing_html(){
            checkHtml(@"
#pragma boo
bml:
    div x,1,cust=test:
        'pre'
        span:'s1'
        span y:'body'
        'post'
",
 "<div class='x' id='1' cust='test'>pre<span>s1</span><span class='y'>body</span>post</div>"
                );
        }


        [Test]
        public void basic_bmlelemnt_test_start_for_developing_html()
        {
            checkHtml(@"
#pragma boo
bml:
    bmlelement div, x,1,cust=test:
        bmlelement span, y:
            output 'body'
",
 "<div class='x' id='1' cust='test'><span class='y'>body</span></div>"
                );
        }

        [Test]
        public void basic_test_start_for_developing_macro()
        {
            checkMacro(@"
#pragma boo
bml:
    div x,1,cust=test:
        span y:
            'body'
",
 @"__write('<div class='x' id='1' cust='test'>')
__write('<span class='y'>')
__write('body')
__write('</span>')
__write('</div>')"
                );
        }

        [Test]
        public void basic_test_start_for_developing_preview()
        {
            var pipe = new Parse();
           /// pipe.InsertAfter(typeof (Boo.Lang.Parser.BooParsingStep), new ExpandBmlStep());
            //pipe.Add(new Boo.Lang.Compiler.Steps.PrintAst());
            TextWriter target = new StringWriter();
            checkMacro(@"
#pragma boo
bml:
    div x,1,cust=test:
        style:
            """"""
    .x {weight:12;}
            """"""

        span y:
            'body'
",
 @"bml :
	bmlelement div, ___start, ___end, x, 1, cust = test:
		bmlelement style, ___start, ___end:
			__write('\r\n    .x {weight:12;}\r\n            ')
		bmlelement span, ___start, ___end, y:
			__write('body')", pipe, Console.Out
                );

        }

        [Test]
        public void strict_mode()
        {
            var pipe = new Parse();
            //pipe.InsertAfter(typeof(Boo.Lang.Parser.BooParsingStep), new ExpandBmlStep());
            //pipe.Add(new Boo.Lang.Compiler.Steps.PrintAst());
            TextWriter target = new StringWriter();
            checkMacro(@"
#pragma boo
bml ex=(div,span):
    div x,1,cust=test:
        style:
            """"""
    .x {weight:12;}
            """"""

        span y:
            'body'
",
 @"bml ex = (div, span):
	bmlelement div, ___start, ___end, x, 1, cust = test:
		style :
			__write('\r\n    .x {weight:12;}\r\n            ')
		bmlelement span, ___start, ___end, y:
			__write('body')", pipe, Console.Out
                );

        }

        [Test]
        public void free_mode()
        {
            //generates xml
            checkHtml(@"
#pragma boo
bml all:
    xml:
        any : 1
        any : 2
        output ""x""
        any s=2 : 3
",
 @"<xml><any>1</any><any>2</any><output class='x'></output><any s='2'>3</any></xml>"
                );

        }

        [Test]
        public void free_mode_with_exceptions()
        {
            //generates xml
            checkHtml(@"
#pragma boo

bml all, ex=(output,):
    xml:
        any : 1
        any : 2
        output ""x""
        any s=2 : 3
",
 @"<xml><any>1</any><any>2</any>x<any s='2'>3</any></xml>"
                );

        }

        [Test]
        public void can_quote_attributes()
        {
            //generates xml
            checkHtml(@"
#pragma boo

bml all, ex=(output,):
    xml:
        any x='&' : 1
        any x='""\'': 2
        any s=2 : '3'
",
 @"<xml><any x='&amp;'>1</any><any x='&quot;&#39;'>2</any><any s='2'>3</any></xml>"
                );

        }

        [Test]
        public void free_mode_with_exceptions_check_macro()
        {
            //generates xml
            checkMacro(@"
#pragma boo

bml all, ex=(output,):
    xml:
        any : 1
        any : 2
        output ""x""
        any s=2 : 3
",
 @"__write('<xml>')
__write('<any>')
__write(1)
__write('</any>')
__write('<any>')
__write(2)
__write('</any>')
__write('x')
__write('<any s='2'>')
__write(3)
__write('</any>')
__write('</xml>')"
                );

        }

       


        [Test]
        public void out_prefixed_are_not_wrapped_in_output()
        {
            //generates xml
            checkMacro(@"
#pragma boo

bml all, ex=(output,):
    xml:
        any : 1
        any : 2
        output ""x""
        any s=2 : outmethod(3)
",
 @"__write('<xml>')
__write('<any>')
__write(1)
__write('</any>')
__write('<any>')
__write(2)
__write('</any>')
__write('x')
__write('<any s='2'>')
outmethod(3)
__write('</any>')
__write('</xml>')"
                );

        }

      

        [Test]
        [Ignore("it was not 'tr' in default exclusions")]
        public void try_to_catch_bug_in_real_view(){
            checkMacro(@"#pragma boo
def outp(role,start,finish):
        sub '/shared/partitions',{@role:role,@itemStart:start,@itemEnd:finish}
def outpd(role):
        outp(role,'<div class=""ws-part"">','</div>')
def outptr(role):
        outp(role,'<tr class=""ws-part""><td class=""ws-part"">','</td></tr>')
def outptd(role):
        outp(role,'<td valign=""top"" class=""ws-part"">','</td>')

section = ""ws-partitions-section""
bml :
	table ""ws-partitions"" :
		tr :
			td id=top, colspan=3, valign=top :
				table @section :
					tr :	outputd(@top)
		tr :
			td id=left, valign=top :
				table @section :
					tr :	outputd(@left)
			td id=main, valign=top :
				div id=maincontent :
					outpd(@header)
					output content
					outpd(@footer)
					outpd(@main)	
			td @section, right, valign=top:
				table : outptr(@right)
		tr :
			td @section, colspan=3, valign=top, id=bottom :
				table :
					tr : outputd(@bottom)",
                                          ""
                );
        }
    }

   
}
