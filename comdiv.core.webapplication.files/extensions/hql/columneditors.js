_result = (function () {
	zeta.hql.selecteditor = Class.create(zeta.hql.columneditorbase,{
		cssquery : ".value_select",
		__setitemvalue : function(item,value){
			item.value = value;
		},
		__getitemvalue : function(item,value){
			return item.value;
		},
		__prepareeditableitem : function(item){
			this.__addonchange(item);
			this.__addretescsupport(item);
		},
	});
	
	zeta.hql.checkboxeditor = Class.create(zeta.hql.columneditorbase,{
		cssquery : ".value_checkbox",
		__setitemvalue : function(item,value){
			if(typeof(value)=="string"){
				value = value == "True" || value == "true";
			}
			item.checked = value;
		},
		__getitemvalue : function(item,value){
			return item.checked;
		},
		__prepareeditableitem : function(item){
			this.__addonchange(item);
			this.__addretescsupport(item);
		},
	});
	
    var result = {
		
		columneditors : [],
		
		preparecutomedits : function(){
			for(i=0;i<this.columneditors.length;i++){
				editor = this.columneditors[i];
				editor.execute();
			}
			new zeta.hql.selecteditor().execute();
			new zeta.hql.checkboxeditor().execute();
		},
		
		lookupedit : function(cell){
			if(cell.editor)return;
			cell.realupdatetarget.hide();
			var e = new Element("input", {style : "width:100%"});
			cell.editor = e;
			cell.appendChild(e);
			type = cell.evalAttr("lookupentity");
			e.focus();
			new Ajax.Autocompleter(cell.editor, "autocompleter", Ajax.siteroot + "/hql/autocomplete.rails", { minChars: 2,
				callback : function(){
					return {
					type : type,
					system : zeta.sql.getCurrentSystem(),
					query : cell.editor.value,
					usecodes : cell.usecodes,
					}
				},
				afterUpdateElement : function(e,en) {
					cell.editor.itemid = en.getAttribute('itemid');
					cell.editor.itemtext = en.getAttribute('itemname');
				},
			});
			
			Event.observe(cell.editor,"keydown",function(e){
				if(e.keyCode==Event.KEY_ESC){
					Event.stop(e);
					zeta.hql.__droplookuper(cell);
					return false;
				}
				
				if(e.keyCode==Event.KEY_RETURN){
					Event.stop(e);
					zeta.hql.applycell(cell,cell.editor.itemid || 0 ,cell.editor.itemtext || "");
					zeta.hql.__droplookuper(cell);
					return false;
				}
			});
		},
		
		__droplookuper : function(cell){
			cell.editor.remove();
			cell.editor = null;
			cell.realupdatetarget.show();
		},
		
		__startlookup :  function(cell,id, type, usecodes){
			type = type.replace(/Proxy[\d\w]+$/,'');
			cell.realupdatetarget = $(cell).down('span');
			cell.oldvalue = id;
			cell.usecodes = usecodes;
			zeta.tableutils.getcolumn(cell).setAttribute('lookupentity', type);
			zeta.hql.lookupedit(cell);
		},

    };
    return result;
});
var zeta = zeta ? zeta : {};
zeta.hql = zeta.hql ? zeta.hql : {};
Object.extend(zeta.hql,_result());