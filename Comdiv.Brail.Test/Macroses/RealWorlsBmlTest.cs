using NUnit.Framework;

namespace Comdiv.Brail.Test
{
  //  [TestFixture] // to prevent TS from loading this test
    public class RealWorldBmlTest : BrailMacroTestBase
    {

       

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


        [Test]
        [Ignore("it's just for testing strange cases")]
        public void try_catch_error_in_bycls()
        {
            checkMacro(@"bml :
	bmlempty :
		if links.has(me as IZetaMainObject,objType,iscls,direction,cls) :
			title
			if list : '<ul>'
			for l in links.GetConvertedLinks(me,cls,iscls,direction,objType) : 
				showLink = true
				g = System.Guid.NewGuid()		
				if list : '<li>'
				if showVerb:
					if verbView :	
						sub @verbView,{@cls:cls, @l:l,@direction:direction}
					else :
						l.Verb
				if ldates :
					@obj.dates(l)
				@verbDelimiter
				'&nbsp;'
				sub objectLink,{@item:l.Object,@_lg:g.ToString(),@_l:l,@_sl:showLink, @objDates :odates}
				_comment as string = ''
				if cType == 1:
					if l.Object and ocomment and l.Object.Comment:
						_comment = _comment + l.Object.Comment + '.'
					if lcomment and l.Comment :
						_comment = _comment + '&nbsp;'+l.Comment
				else :
					if lcomment and l.Comment:
						_comment  = _comment + l.Comment+ '.'
					if l.Object and ocomment and l.Object.Comment:
						_comment = _comment + '&nbsp;'+l.Object.Comment
				_comment = /(&nbsp;)+\s*$/.Replace(_comment.Trim(),'')
				_comment = /\.+/.Replace(_comment,'.');
				if _comment :
					'.&nbsp;'
					@_comment
				if zeta.Actual(l,'0DLCMT') and lcomment:
					br
					@cm.getText(zeta.Actual(l, '0DLCMT'))
				if advView :
					sub @advView,{@l : l}
				'.'
				if list : '</li>'
			if list : '</ul>'",
                                          ""
                );
        }
        [Test]
       [Ignore("it was not 'tr' in default exclusions")]
        public void autocomplete(){
            checkMacro(@"#pragma boo
defines :
	id as string = """"
	index as string = """"
	action as string = siteroot+'/database/object/aclist.rails'
	target as string = ""${id}_item${index}""
	itype as string = ""hidden""
	type as string = ""NONE""
	codes as string = """"
	custom as string = """"
	cls as string = """"
	style as string = """"
	callback as string = """"
	autoclear as string = """"
	params as string = ( ""&ajax=1&type={0}&usecodes={1}&custom={2}"" % (type, codes  , custom ))

mytarget = ( target == ""${id}_item${index}"" )


bml :
	input @cls,""${id}${index}"",  autocomplete=off, name=prefix, style= ""z-index:1000;${style}""
	if mytarget:
		input type=@itype, id= ""${id}_item${index}"",  name=@id,  value= """"
	div id= ""${id}__${index}"",  'class'=auto_complete, style= ""display:none""
	script : """"""
  new Ajax.Autocompleter('${id}${index}', 'main-autocompleter', '${action}',{
  parameters: '${params}',""""""
		if callback :
			""callback : ${callback}""
		""""""
  afterUpdateElement : function(e,en){
			if(Data&&Data.AfterUpdateAutoComplete)Data.AfterUpdateAutoComplete(e,en);
			$('${target}').value = en.getAttribute('item_id');""""""
		if autoclear:
			""""""$('${id}${index}').update('');""""""
		""""""}
		})""""""
	",
                                          ""
                );
        }
    }

    
}
