using Comdiv.Extensibility;
using NUnit.Framework;

namespace Comdiv.Brail.Test
{
    [TestFixture]
    public class BmlTemplatingTest : BrailMacroTestBase
    {

      
        [Test]
        public void basics()
        {
            checkHtml(@"
#pragma boo
bml:
    template mydiv:
        div 'class'=mydiv
    mydiv
    mydiv
",
 @"<div class='mydiv'></div><div class='mydiv'></div>"
                );
        }

		[Test]
		public void bug_NOPROTOTYPE()
		{
			checkHtml(@"
#pragma boo
bml:
    template mydiv:
        div onclick=""$('x')""
    mydiv
    mydiv
",
 @"<div onclick='$(&#39;x&#39;)'></div><div onclick='$(&#39;x&#39;)'></div>"
				);
		}


        

        [Test]
        public void expand_text_nodes_in_template()
        {
            checkHtml(@"
#pragma boo
x = 1
bml:
    template mydiv:
        div 'class'=mydiv :
            'string'
            @x
    mydiv
",
 @"<div class='mydiv'>string1</div>"
                );
        }


        [Test]
        public void basics_with_includeast()
        {
            checkHtml(@"
#pragma boo
bml:
    includeast Comdiv.Brail.Test.mydivtemplate, Comdiv.Brail.Test
    mydiv
    mydiv2
",
 @"<div class='mydiv'></div><div id='myid'></div>"
                );
        }

        [Test]
        public void with_substitutions()
        {
            checkHtml(@"
#pragma boo
x = 2
bml:
    template hidden:
        input type=hidden, id=@_1, name=@_1, value=@_2
    hidden foo
    hidden bar, @x
",
 @"<input type='hidden' id='foo' name='foo' value=''></input><input type='hidden' id='bar' name='bar' value='2'></input>"
                );
        }

        [Test]
        public void with_substitutions_complex()
        {
            checkHtml(@"
#pragma boo
x = 2
bml:
    template hidden:
        bmlempty: 
            bmlelement h1 : output ""the value for ${@_1_} is '${@_2_}'"" 
            bmlelement input, type=hidden, id=@_1, name=@_1, value=@_2
        
    hidden foo
    hidden bar, @x
",
 @"<h1>the value for foo is ''</h1><input type='hidden' id='foo' name='foo' value=''></input><h1>the value for bar is '2'</h1><input type='hidden' id='bar' name='bar' value='2'></input>"
                );
        }

        [Test]
        public void expands_bml_in_template()
        {
            checkHtml(@"
#pragma boo
x = 2
bml:
    template hidden:
        bmlempty: 
            h1 : output ""the value for ${@_1_} is '${@_2_}'"" 
            input type=hidden, id=@_1, name=@_1, value=@_2
        
    hidden foo
    hidden bar, @x
",
 @"<h1>the value for foo is ''</h1><input type='hidden' id='foo' name='foo' value=''></input><h1>the value for bar is '2'</h1><input type='hidden' id='bar' name='bar' value='2'></input>"
                );
        }


        [Test]
        public void can_use_multiple_root_elements()
        {
            checkHtml(@"
#pragma boo
x = 2
bml:
    template hidden:
        h1 : ""the value for ${@_1_} is '${@_2_}'"" 
        input type=hidden, id=@_1, name=@_1, value=@_2
        
    hidden foo
    hidden bar, @x
",
 @"<h1>the value for foo is ''</h1><input type='hidden' id='foo' name='foo' value=''></input><h1>the value for bar is '2'</h1><input type='hidden' id='bar' name='bar' value='2'></input>"
                );
        }

        [Test]
        public void named_parameters_with_defaults()
        {
            checkHtml(@"/* test that pragma boo not need to be first comment*/

bml:
    template hidden:
        input type=(@_type,hidden), id=@_1, value=@_2
        
    hidden foo
    hidden bar, type=myhidden, val
#pragma boo
",
 @"<input type='hidden' id='foo' value=''></input><input type='myhidden' id='bar' value='val'></input>"
                );
        }

        [Test]
        public void free_parameter_list_binder()
        {
            checkHtml(@"/* test that pragma boo not need to be first comment*/

bml:
    template hidden:
        input id=@_1, value=@_2 , @__
    hidden bar, type=myhidden, val
#pragma boo
",
 @"<input id='bar' value='val' type='myhidden'></input>"
                );
        }


        [Test]
        public void binding_parameters_to_child_element()
        {
            checkHtml(@"/* test that pragma boo not need to be first comment*/

bml:
    template test:
        div id=@_1 :
            div id=""${@_1_}_sub"", @_sub
    test bar, sub=(custom=test)
#pragma boo
",
 @"<div id='bar'><div id='bar_sub' custom='test'></div></div>"
                );
        }


        [Test]
        public void can_use_default_values()
        {
            checkHtml(@"/* test that pragma boo not need to be first comment*/

bml:
    template test:
        div value=(@_1,'hello')       
    test
    test foo
#pragma boo
",
 @"<div value='hello'></div><div value='foo'></div>"
                );
        }

        public void so_using_on_declarations()
        {
            checkHtml(@"
bml:
    template hidden:
        input type=hidden, id=@_1, name=@_1, value=@_2
    template labeled_hidden :
        bmlempty :
            bmlelement h1 : output ""it's hidden for '${@_1_}' with name '${@_2_}'""
            bmlelement hidden, @_2, @_3
        
    labeled_hidden label, name, val

",
 @"<h1>it's hidden for 'label' with name 'name'</h1><input type='hidden' id='name' name='name' value='val'></input>"
                );
        }

        [Test]
        public void nesting_at_rendering()
        {
            checkHtml(@"
#pragma boo
bml:
    template mydiv:
        div 'class'=mydiv, @__:
            BODY
    mydiv id=1:
        mydiv id=2
",
 @"<div class='mydiv' id='1'><div class='mydiv' id='2'></div></div>"
                );
        }

        [Test]
        public void nesting_at_rendering_with_bodies()
        {
            checkHtml(@"
#pragma boo
bml:
    template mydiv:
        div 'class'=mydiv,@__ :
            output 'test'
            BODY
    mydiv id=1:
        mydiv id=2
",
 @"<div class='mydiv' id='1'>test<div class='mydiv' id='2'>test</div></div>"
                );
        }


          }

    public class mydivtemplate : BooCodeBasedAstIncludeSource{
        public mydivtemplate(){
            this.Code = @"template mydiv:
    div 'class'=mydiv
template mydiv2:
    div id=myid";
        }
    }
}
