// Copyright 2007-2009 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// MODIFICATIONS HAVE BEEN MADE TO THIS FILE

using System;
using MvcContrib.Comdiv.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    [TestFixture]
    [Category("Brail")]
    public class ForEachMacroTest : BrailMacroTestBase{
        [Test]
        [Ignore("Was only for debug")]
        public void FIX_strange_behaviour(){
            checkMacro(
                @"#pragma boo
foreach i as ThemaConfiguration = configs :
    astable :
        heads 'Code', 'Name', 'Import' , 'Active'
        cells i.Code,i.Name
        cell :
            foreach th = i.Imports :
                output th.Code
        cell :
            i.Active
            input type=button, value='Изменить', onclick=""Zeta.thema.changeactive('${i.Code}')""",
                "");
        }

        
[Test]
        public void nested_foreachs_works_well_fix_parts_catching_on_nested(){
            checkHtml(@"#pragma boo
foreach i=(1,2):
    beforeall 'parent'
    foreach j=(3,4): 
        output ""${i}_${j},""
        beforeall 'nested'" , "parentnested1_3,1_4,nested2_3,2_4,");
        }
        [Test]
        public void can_use_default_block_instead_of_onitem(){
            string code =
                @"
import MvcContrib.Comdiv.Brail
foreach items:
    out_on_item()
    beforeall:
        out_before_all()
    onerror:
        out_on_error()
    between:
        out_between()
    afterall:
        out_afterall()
    beforeeach:
        out_beforeeach()
    aftereach:
        out_aftereach()
    onempty:
        out_onempty()
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	out_before_all()
	for i in current_collection:
		if _idx > 0:
			out_between()
		out_beforeeach()
		try:
			out_on_item()
		except _ex as System.Exception:
			out_on_error()
		out_aftereach()
		++_idx
	then:
		out_afterall()
else:
	out_onempty()
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void can_use_inline_default_body_call()
        {
            checkMacro(@"#pragma boo
foreach params : ""<tr><td>${i.Code}</td><td>${i.Name}</td></tr>""
", @"current_collection = _wrapcollection(params)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for i in current_collection:
		__write('<tr><td>$(i.Code)</td><td>$(i.Name)</td></tr>')
		++_idx");
        }

        [Test]
        public void asolist_snippet(){
            string code =
                @"<%
foreach i=(1,2,3):
    asolist mylist, li=(id=""li_${_idx}"")
end
%>
";
            string html =
                @"<ol class='mylist'><li id='li_0'>1</li><li id='li_1'>2</li><li id='li_2'>3</li></ol>";
            // checkMacro(code, "");
            checkHtml(code, html);
        }

        [Test]
        public void aspars_snippet(){
            string code =
                @"
<%foreach 1:
    aspars c1
end%>
";
            string expectedresult =
                @"<p class='c1'>1</p>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void asrows_snippet(){
            string code =
                @"
<%foreach i=(1,2):
    asrows c1
end%>
";
            string expectedresult =
                @"<span class='c1'>1</span><br/><span class='c1'>2</span>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void asselect_snippet(){
            string code =
                @"
<%foreach i=1:
    asselect name=myselect,option=(@i,custom=x)
end
%>
";
            string expectedresult =
                @"<select name='myselect'><option value='1' custom='x'>1</option></select>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void null_partitions_allowed_FOR_COVERAGE()
        {
            string code = @"#pragma boo
foreach i=(1,2,3):
    beforeall";
            checkHtml(code, "123");
        }

        [Test]
        public void foreach_without_params_should_throw_valid_execption_FOR_COVERAGE(){
            string code = @"#pragma boo
foreach:
    outsome()";
            TestDelegate a = () => checkMacro(code, "");
            var ex = Assert.Throws<Exception>(a);
            StringAssert.Contains("need at least one IEnumerable parameter",ex.Message);

        }
        [Test]
        public void astable_head_submacro()
        {
            checkHtml(@"#pragma boo
foreach i=(1,2,3):
    astable:
        head ""x""
        head :
            output 'z'
            output ""${2}""
        heads '1','2'
        heads:
            '1'
            output 2
", "<table><thead><tr><th>x</th><th>z2</th><th>1</th><th>2</th><th>1</th><th>2</th></tr></thead><tbody><tr><td>1</td></tr><tr><td>2</td></tr><tr><td>3</td></tr></tbody></table>");
        }

        [Test]
        public void astable_heads_made_cells_complete(){
            string code = @"#pragma boo
foreach 1:
    astable:
        heads '1*1','1*2'
        cells i
";
            string expectedresult =
                @"<table><thead><tr><th>1*1</th><th>1*2</th></tr></thead><tbody><tr><td>1</td><td></td></tr></tbody></table>";
            //checkMacro(code, "");
                 checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_heads_made_heads_complete(){
            const string code = @"#pragma boo
foreach 1:
    astable:
        heads '1*1'
        cells i,i,i,i
";
            const string expectedresult = @"<table><thead><tr><th>1*1</th><th></th><th></th></tr></thead><tbody><tr><td>1</td><td>1</td><td>1</td><td>1</td></tr></tbody></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_heads1(){
            const string code = @"#pragma boo
foreach 1:
    astable:
        heads '1*1','1*2'
        cells i,i*2
";
            string expectedresult =
                @"<table><thead><tr><th>1*1</th><th>1*2</th></tr></thead><tbody><tr><td>1</td><td>2</td></tr></tbody></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_snippet(){
            string code = @"#pragma boo
foreach 1:
    astable:
        cell i
";
            string expectedresult =
                @"<table><tr><td>1</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_snippet_block_cell()
        {
            string code = @"#pragma boo
foreach 1:
    astable:
        cell:
            if 1==2 :
                output 'false'
            else:
                output 'true'
";
            string expectedresult =
                @"<table><tr><td>true</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_snippet_block_cells()
        {
            string code = @"#pragma boo
foreach 1:
    astable:
        cells:
            (i,)
            (i*2,)
";
            string expectedresult =
                @"<table><tr><td>1</td><td>2</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_snippet_with_classname_and_id()
        {
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    astable cls,theid
";
            string expectedresult =
                @"<table class='cls' id='theid'><tr><td>1</td></tr><tr><td>2</td></tr><tr><td>3</td></tr></table>";
            checkHtml(code, expectedresult);
        }


        [Test]
        public void astablerows_snippet()
        {
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    astablerows :
        cells i, i
";
            string expectedresult =
                @"<tr><td>1</td><td>1</td></tr><tr><td>2</td><td>2</td></tr><tr><td>3</td><td>3</td></tr>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astablerows_snippet_simple()
        {
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    astablerows
";
            string expectedresult =
                @"<tr><td>1</td></tr><tr><td>2</td></tr><tr><td>3</td></tr>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astablerows_in_table_snippet()
        {
            string code =
                @"#pragma boo
foreach i=(1,2):
    astable
    beforeeach """"
    aftereach """"
    foreach j=(3,4):
        astablerows  row=('x'):
            cell j
";
            string expectedresult =
                @"<table><tr class='x'><td>3</td></tr><tr class='x'><td>4</td></tr><tr class='x'><td>3</td></tr><tr class='x'><td>4</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        
        [Test]
        public void astable_snippet_with_attributes()
        {
            string code =
                @"#pragma boo
foreach i=(1,):
    astable mytable, @theid, {myattr : myvalue, myattr2 : ""my value 2"", @mydinattr : 34}
";
            string expectedresult =
                @"<table class='mytable' id='23' myattr='myvalue' myattr2='my value 2' test='34'><tr><td>1</td></tr></table>";
            checkHtml(code,new{theid=23, mydinattr="test"}, expectedresult);
        }

        [Test]
        public void astable_snippet_with_row_attributes(){
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    astable row=(cls,""theid${i}"")
";
            string expectedresult =
                @"<table><tr class='cls' id='theid1'><td>1</td></tr><tr class='cls' id='theid2'><td>2</td></tr><tr class='cls' id='theid3'><td>3</td></tr></table>";
            checkHtml(code,  expectedresult);
        }

        [Test]
        [Ignore("Вроде проверено навека")]
        public void bug_astable_snippet_with_row_attributes_and_heads()
        {
            string code =
                @"#pragma boo
foreach i=(1,2,3) :
    astable row=(""installed_${i}"") :
	    heads ""Имя"", ""Установлен"", ""Команды""
	    cells i.Name, i
	    cell :
		    if i.IsInstalled :
			    input type=button, onclick=""comdiv.extensions.install('${i}')"", value=""Обновить""
			    input type=button, onclick=""comdiv.extensions.uninstall('${i}')"", value=""Удалить""
		    else :
			    input type=button, onclick=""comdiv.extensions.install('${i}')"", value=""Установить""
";
            string expectedresult =
                @"<table><tr class='cls' id='theid1'><td>1</td></tr><tr class='cls' id='theid2'><td>2</td></tr><tr class='cls' id='theid3'><td>3</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_snippet_with_row_attributes_test2()
        {
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    astable row=(cls,""theid${i}""):
        cells 3,4
";
            string expectedresult =
                @"<table><tr class='cls' id='theid1'><td>3</td><td>4</td></tr><tr class='cls' id='theid2'><td>3</td><td>4</td></tr><tr class='cls' id='theid3'><td>3</td><td>4</td></tr></table>";
            checkHtml(code, expectedresult);
        }



        [Test]
        [Ignore("Not well formed")]
        public void astable_snippet_with_row_attributes_test3()
        {
            
            string code =
                @"#pragma boo
bml :
	h1 : '${obj.Name}'
	h2 : 'Телефонный справочник АССОИ УГМК'
	num = 0
	foreach users :
		prepare :
			num += 1
		astable row=(""admin_${i.Boss}"",""row_${i.Id}""):
			heads '№', 'ФИО', 'Тел.', 'Должность', 'e-mail', 'Ф', 'Т', 'С', 'П', 'И'
			cells num, i.Name, i.Contact, i.Dolzh, i.Comment, '', '', '', '', ''
";
            string expectedresult =
                @"<table><tr class='cls' id='theid1'><td>3</td><td>4</td></tr><tr class='cls' id='theid2'><td>3</td><td>4</td></tr><tr class='cls' id='theid3'><td>3</td><td>4</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        
        [Test]
        public void astable_snippet_with_row_attributes_can_expand_array_literals()
        {
            
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    astable row=[cls,""theid${i}""]
";
            string expectedresult =
                @"<table><tr class='cls' id='theid1'><td>1</td></tr><tr class='cls' id='theid2'><td>2</td></tr><tr class='cls' id='theid3'><td>3</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_snippet2(){
            string code =
                @"#pragma boo
class test:
    def hoho() as string:
        return 'ddd'
foreach 1:
    astable:
        cell i
        cell i*2,colspan=2
        cell test().hoho()
";
            string expectedresult =
                @"<table><tr><td>1</td><td colspan='2'>2</td><td>ddd</td></tr></table>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void astable_snippet2_cells_used(){
            string code =
                @"#pragma boo
class test:
    def hoho() as string:
        return 'ddd'
foreach 1:
    astable:
        cells i,(i*2,colspan=2), test().hoho()
";
            string expectedresult =
               @"<table><tr><td>1</td><td colspan='2'>2</td><td>ddd</td></tr></table>";
            checkHtml(code, expectedresult);
        }


        [Test]
        public void asulist_snippet(){
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    asulist mylist
";
            string expectedresult =
                @"<ul class='mylist'><li>1</li><li>2</li><li>3</li></ul>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void asulist_snippet_li_dyn_class()
        {
            string code =
                @"#pragma boo
foreach i=(1,2,3):
    asulist mylist,li=(""licls_${_idx % 2}"")
";
            string expectedresult =
                @"<ul class='mylist'><li class='licls_0'>1</li><li class='licls_1'>2</li><li class='licls_0'>3</li></ul>";
            checkHtml(code, expectedresult);
        }

        [Test]
        public void can_use_method_out_parameters(){
            string code =
                @"
import MvcContrib.Comdiv.Brail
foreach items:
    beforeall beforeall()
    onitem onitem()
    onerror onerror()
    between between()
    afterall afterall()
    beforeeach beforeach()
    aftereach aftereach()
    onempty onempty()
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	beforeall()
	for i in current_collection:
		if _idx > 0:
			between()
		beforeach()
		try:
			onitem()
		except _ex as System.Exception:
			onerror()
		aftereach()
		++_idx
	then:
		afterall()
else:
	onempty()
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void can_use_string_parameters(){
            string code =
                @"
import MvcContrib.Comdiv.Brail
foreach items:
    beforeall 'beforeall'
    onitem 'onitem'
    onerror 'onerror'
    between 'between'
    afterall 'afterall'
    beforeeach 'beforeach'
    aftereach 'aftereach'
    onempty 'onempty'
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	__write('beforeall')
	for i in current_collection:
		if _idx > 0:
			__write('between')
		__write('beforeach')
		try:
			__write('onitem')
		except _ex as System.Exception:
			__write('onerror')
		__write('aftereach')
		++_idx
	then:
		__write('afterall')
else:
	__write('onempty')
";
            checkMacro(code, expectedresult);
        }


        [Test]
        public void change_idx_var_name(){
            string code = @"
import MvcContrib.Comdiv.Brail
foreach items,myidx:
    outsome()
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	myidx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for i in current_collection:
		outsome()
		++myidx
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void default_body(){
            string code = @"
foreach i=(1,2,3)";
            string expectedresult =
                @"current_collection = _wrapcollection((1, 2, 3))
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for i in current_collection:
		__write(i)
		++_idx
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void extract_iteration_var_name(){
            string code = @"
import MvcContrib.Comdiv.Brail
foreach current=items:
    outsome()
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for current in current_collection:
		outsome()
		++_idx
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void full_notation(){
            string code =
                @"
import MvcContrib.Comdiv.Brail
foreach items:
    prepare :
        some_code_1()
        some_code_2()
    beforeall:
        out_before_all()
    onitem:
        out_on_item()
    onerror:
        out_on_error()
    between:
        out_between()
    afterall:
        out_afterall()
    beforeeach:
        out_beforeeach()
    aftereach:
        out_aftereach()
    onempty:
        out_onempty()
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	out_before_all()
	for i in current_collection:
		if _idx > 0:
			out_between()
		some_code_1()
		some_code_2()
		out_beforeeach()
		try:
			out_on_item()
		except _ex as System.Exception:
			out_on_error()
		out_aftereach()
		++_idx
	then:
		out_afterall()
else:
	out_onempty()

";
            checkMacro(code, expectedresult);
        }

        [Test(Description = "here we test our posibility to embed interpolation in macroses")]
        public void html_test(){
            Assert.AreEqual("<table class='mytable' id='123' custom='124'><tr><td>1</td></tr><tr><td>2</td></tr><tr><td>3</td></tr></table>", MyBrail._Process(
                @"<%
                    foreach i=(1,2,3)            :
                        astable mytable,@x,{custom:x+1}
                        %><td>${i}</td><%
                    end
                    %>"
                , new{x = 123}));
        }

        [Test]
        public void minimal(){
            string code = @"
foreach items
";
            string expectedresult =
                @"
current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for i in current_collection:
		__write(i)
		++_idx
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void new_features_20091225(){
            string code =
                @"#pragma boo  			
foreach 1:    									
    astable count=current_collection.Count:		
        cell i  								
        cell i*2,colspan=2  					

x as System.Object = 'dSSS'						
foreach i as string = (x,x,x):					
	output i.Substring(0,1)						
	
foreach i=('1;','2;','3;')						

foreach null:									
	beforeall 'start'
	onempty proceed";
            string html = "<table count='1'><tr><td>1</td><td colspan='2'>2</td></tr></table>ddd1;2;3;start";
            checkHtml(code, html);
        }

        [Test]
        public void woks_with_bml()
        {
            string code =
                @"#pragma boo  			
foreach i=(' ya- ','hoo'):    									
    beforeall:
        bml:
            span mycls, myid : 'hello world!'					
";
            string html = "<span class='mycls' id='myid'>hello world!</span> ya- hoo";
            checkHtml(code, html);
        }

        [Test]
        public void no_item_error_handling_mode(){
            string code =
                @"
import MvcContrib.Comdiv.Brail
foreach items:
    beforeall beforeall()
    onitem onitem()
    between between()
    afterall afterall()
    beforeeach beforeach()
    aftereach aftereach()
    onempty onempty()
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	beforeall()
	for i in current_collection:
		if _idx > 0:
			between()
		beforeach()
		onitem()
		aftereach()
		++_idx
	then:
		afterall()
else:
	onempty()
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void partitions_are_nested_macroses(){
            //test that beign standalone - they not treated as macroses
            checkMacro(
                @"
beforeall:
    a = 0
afterall:
    a = 0
onitem:
    a = 0
beforeach:
    a = 0
aftereach:
    a = 0

",
                @"
beforeall({ a = 0 })
afterall({ a = 0 })
onitem({ a = 0 })
beforeach({ a = 0 })
aftereach({ a = 0 })
");
        }

        [Test]
        public void proceed_on_empty_behaviour_mode(){
            string code = @"
foreach items:
    onempty proceed
";
            string expectedresult =
                @"current_collection = _wrapcollection(items)
___proceed = true
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for i in current_collection:
		__write(i)
		++_idx
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void simple_mode(){
            string code = @"
foreach items:
    outsome()
";
            string expectedresult =
                @"
current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for i in current_collection:
		outsome()
		++_idx
";
            checkMacro(code, expectedresult);
        }

        [Test]
        public void with_explicite_type(){
            string code = @"
foreach i as int =items
";
            string expectedresult =
                @"
current_collection = _wrapcollection(items)
___proceed = false
if ___proceed or (not isempty(current_collection)):
	_idx = 0
	if null == current_collection:
		current_collection = _wrapcollection((,))
	for i as int in current_collection:
		__write((i as int))
		++_idx
";
            checkMacro(code, expectedresult);
        }
    }
}