/*
//Etable v0.8
// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE

	
	
	REQUIRES: 
	1) prototype.js
	2) table with normal <thead> with <th> (optional) and <tbody>
	3) etable2.css (can be replaced with your styles)
	
	Features:
	1) column sorting (+skiping, string and number mode)
	2) filtering (sets of values, conjuncions)
	3) formulas (direct targetting, absolute addresses, relative addresses, sum up-left-right-down)
	4) can work with simple <td>, with inputs, nested tables
	5) keyboard navigation over input|select elements a-la Excel
	
	Using :
		new comdiv.ui.Etable(element) where element - string or reference to <table> element
*/


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//					COMMON UTILITIES
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////


var $ = $ || (function(e){if (typeof(e)=='object')return e; return document.getElementById(e);});
var Selector = Selector || null;

Object.extend(Element.Methods, {
    getText: function(element) {
		
        element = $(element);
		var attr = element.getAttribute("value");
		if(attr) return attr;
        return undefined == element.textContent ? element.innerText : element.textContent;
    },
	setAttr : function(element,name,value){
		if(undefined == (name || value))return element;
		element = $(element);
		element.setAttribute(name,value);
		return element;
	}
})
Element.addMethods();

Object.extend(Enumerable,{
	intersect: function(en){
		var result = [];
		this.each(function(e){
				for (var j in  en){
					if (e == en[j]){
						result.push(en[j]);
					}
				}
		});
		return result;
	},
	firstOrDefault : function(){
		if(this.length==0)return null;
		return this.first();
	},
	
});

Object.extend(Array.prototype, Enumerable);
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//			CLASS DEFINITION
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
var comdiv = (typeof(comdiv) == 'undefined') ? {} : comdiv;
if(typeof(comdiv.ui) == 'undefined'){
  comdiv.ui = {};
}
comdiv.ui.Etable = Class.create();
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//etable statically keep last initialized etable which performs linked list
//no much needs to iterate through tables, but navigation requires prev/next moving between tables
comdiv.ui.Etable.current = null;
comdiv.ui.Etable.rowcache = {};


Object.extend(comdiv.ui.Etable.prototype, {
	DATATYPES : {
		STRING : "string",
		NUMBER : "number"
	},
	//initializes etable facility on <table> element
    initialize: function(element, options) {
		//setup linked list
		this.prev = comdiv.ui.Etable.current;
		if(this.prev!=null){
			this.prev.next = this;
		}
		comdiv.ui.Etable.current = this;
		
		//prepare defaults and two-way bind of etable object to <table> element
		this.element = $(element);
		this.element.etable = this;
		this.id = this.element.getAttribute("id");
		//columns descriptors are not prepared by default
		this.columns = [];
        this.options = Object.extend( {
			//sorting is used by default
			sort : true,
			//headers and columns are processed by default
			usecolumns: true
		} , (options || {}));
		//body is main part of table - we try to get it directly by attribute...
		try{
		this.body = Selector.findChildElements(this.element, ["tbody[tableBody]"]).reverse()[0];
		}catch(e){
		}
		if(!this.body){
			//or use default body
			this.body = Selector.findChildElements(this.element, ["tbody"]).reverse()[0];
		}//we process bodies such way because there are table with several headers and bodies (for now just one of them can be used with etable)
		//rows are finded by special attribute (so we can use nested tables without overhead)
		
		//body is main part of table - we try to get it directly by attribute...
		this.head = Selector.findChildElements(this.element, ["thead[tableHead]"]).reverse()[0];
		if(!this.head){
			//or use default body
			this.head = Selector.findChildElements(this.element, ["thead"]).reverse()[0];
		}
		if(!this.head){
			this.head = this.body;
		}
		
		this.rows = comdiv.ui.Etable.rowcache[this.element.getAttribute('id')];
		if(!this.rows){
			this.rows = Selector.findChildElements(this.body, ["tr[tableRow]"]);
			if(!this.rows[0]){
				//or we use all <tr> under body (for simple tables without nesting)
				this.rows = Selector.findChildElements(this.body, ["tr"]);
			}
		}
		//formuls must be prepared here further and they are layered to 5 layers to allow 
		//to definde cascade dependencies of formula evaluation without formula analyzing
		this.formuls = {
			0 : [],
			1 : [],
			2 : [],
			3 : [],
			4 : []
		}
		
		//if we must process header we do it
		if(this.options.usecolumns){
			// in this mode we process all headers and found information about cell behaviour under this column
			// and setting up sorting, filtering and other stuff on headers
			var _headrow = Selector.findChildElements(this.head,["tr",])[0];
			var _cols = Selector.findChildElements(_headrow, ["th",]);
			
			this.columns =  _cols.collect(
				function(e,idx){
					e.columnDataType = e.getAttribute("columnDataType") || this.DATATYPES.STRING;
					e.columnIndex = idx;
					e.etable = this;
					e.sortOrder = 0;
					e.setAttr("sortOrder",0);
					e.filterType = Number ( e.getAttribute("columnForFilter") || 0);
					e.skipInOrder = e.getAttribute("columnNoSort") || false;
					e.formula  = e.getAttribute("formula") || "";
					e.flayer = e.getAttribute("flayer") || "";
					if(e.filterType){
						var filterDiv = new Element("div")
							 .setAttr("id","fd_"+this.id+"_"+e.columnIndex)
							 .addClassName("etable_filter_zone")
			
							 ;
						
						 var filterButton = new Element("span").addClassName("etable_filter_span")
								.setAttr("title","Фильтр").update("&nbsp;&nbsp;&nbsp;");
						 Event.observe(filterButton,"click",(function(){
								this._closeFilters(e);
								filterDiv.toggle();
							}).bind(this));
						 e.filter = filterDiv;
						 e.insert({top:filterButton});
						 e.insert({bottom:filterDiv} );
						 filterDiv.toggle();
					}
					if(this.options.sort && !e.skipInOrder){
						sorter = new Element("span").addClassName("etable_sort_span")
								.setAttr("title","Сортировать").update("&nbsp;&nbsp;&nbsp;");
						Event.observe(sorter,"click",(function(){e.etable.sort(e);}).bind(this));
						e.insert({top:sorter});
					}
					return e;
				},this
			);
		}
		
		
		this.firstinput = null;
		this.lastinput = null;
		
		
		for (var ridx=0; ridx < this.rows.length; ridx++){
					var r = this.rows[ridx];
					r.formula = r.getAttribute("formula") || "";
					r.afterformula = r.getAttribute("afterformula") || "";
					
					r.flayer = r.getAttribute("flayer") || "";
					r.sortKey = r.getAttribute("sortKey",0);
					r.tableCells = Selector.findChildElements(r,["td[tablecell],"]);
					if(!r.tableCells[0]){
						r.tableCells = Selector.findChildElements(r,["td"]);
					}
					
					r.tableCells.each(function(c,cidx){
						c.cellId = this.id + "_cell_"+ridx+"_"+cidx;
						c.setAttr("cellid",c.cellId);
						c.flayer = Number(c.getAttribute("flayer")) || r.flayer || (this.columns[cidx] ? this.columns[cidx].flayer : 0) || 0;
						c.formula = c.getAttribute("formula") || r.formula || (this.columns[cidx] ? this.columns[cidx].formula : "" ) || "";
						c.afterformula = c.getAttribute("afterformula") || r.afterformula ;
						
						if (c.afterformula != ""){
							c.afterformula = eval(c.afterformula);
						}else{
							c.afterformula = function(cell,val){};
						}
						c.ridx = ridx;
						c.cidx = cidx;
						
						try{
							
							var input = Selector.findChildElements(c,["input[tablevalue]","select[item_id]","input[oldvalue]"]);
							c.input = null;
							if(input.length > 0){
								c.input = input[0];
								c.input.td = c.input.up("td.data");
								c.input.tr = c.input.up("tr[tablerow]");
								c.input.tr.childElements().each(function(e,i){
									if(e===c.input.td){
										c.input.colindex = i;
									}
									
								});
								
								if(this.firstinput==null){
									this.firstinput = c.input;
								}
								this.lastinput = c.input;
								c.input.cidx = c.cidx;
								c.input.ridx = c.ridx;
								Event.observe(c.input,"change",(function(){
									this.evaluate();
								}).bind(this));
								Event.observe(c.input,"keyup",(function(event){
									this._onkeydown(event);
								}).bind(this));
								Event.observe(c.input,"focus",function(event){
									c.input.tr.childElements().each(function(e){
										e.addClassName("etable_cell_focused");
									});
									c.input.tr.siblings().each(function(e){
										if(e.childElements().length>c.input.colindex){
											e.childElements()[c.input.colindex].addClassName("etable_cell_focused");
										}
										
									});
									var head = c.input.tr.up("table").down("thead").down("tr");
									if(head){
										if(head.childElements().length>c.input.colindex){
											head.childElements()[c.input.colindex].addClassName("etable_cell_focused");
										}
									}
									
									c.input.addClassName("etable_cell_focused");
								});
								Event.observe(c.input,"blur",function(event){
									c.input.tr.childElements().each(function(e){
										e.removeClassName("etable_cell_focused");
									});
									
									c.input.tr.siblings().each(function(e){
										if(e.childElements().length>c.input.colindex){
											e.childElements()[c.input.colindex].removeClassName("etable_cell_focused");
										}
									});
									
									var head = c.input.tr.up("table").down("thead").down("tr");
									if(head){
										if(head.childElements().length>c.input.colindex){
											head.childElements()[c.input.colindex].removeClassName("etable_cell_focused");
										}
									}
									
									c.input.removeClassName("etable_cell_focused");
								});
							}
						}catch(e){
						}
						if(c.formula){
							c.parsedFormula = this._parse(ridx,cidx,c.formula);
							this.formuls[c.flayer].push(c);
						}
					},this);
					r.orders = {}
				}
		this.currentRows = this.rows;		
		this.evaluate();
		
		
		
    },
	
	_onkeydown : function(event){
		var i = $et(event);
        var result = true;
        var direction = 'down';
        
		if (event.keyCode==13) {
			result = false;
		}else{
			if(ETable.OverrideKeyDown){
				if (event.keyCode==40) result = false;
				else if (event.keyCode==38) {direction='up'; result = false;}
				else if (event.keyCode==37) {direction='left'; result = false;}
				else if (event.keyCode==39) {direction='right'; result = false;}
			}
		}
        if (result) return result;
        
		this._domove(i, direction);
		
		
        event.stop();
        return false;
	},
	
	_domove : function(e, dir){
		if(dir == "down"){
			this._movedown(e);
		}
		else if (dir == "up"){
			this._moveup(e);
		}
		else if (dir == "right"){
			this._moveright(e);
		}
		else{
			this._moveleft(e);
		}
	},
	
	_getinput :function(r,c){
			var cell = this._getcell(r,c);
			if(null==cell) return null;
			return cell.input;
			
	},
	
	
	_movedown : function(e){
		//if(e.ridx == (this.rows.length - 1))return;
		var targetinput = null;
		for(var i=e.ridx+1; i<this.rows.length; i++){
			if (null!=(targetinput = this._getinput(i,e.cidx))){
				this._moveto(targetinput);
				break;
			}
		}
		if(null==targetinput){
			if(this.next!=null){
				if(this.next.firstinput!=null){
					this._moveto(this.next.firstinput);;
				}
			}
		}
	},
	
	_moveto : function(e){
		var val = e.value;
		e.focus();
		
		//HACK: workaround firefox problem on focusing select elements (propagates keydown always)
		if(e.nodeName =="SELECT" ){
			window.setTimeout(function(){e.value = val;},50);
		}
	},
	
	_moveup : function(e){
		//if(e.ridx == 0) return;
		var targetinput = null;
		for(var i=e.ridx-1; i>=0; i--){
			if (null!=(targetinput = this._getinput(i,e.cidx))){
				this._moveto(targetinput);
				break;
			}
		}
		
		if(null==targetinput){
			if(this.prev!=null){
				if(this.prev.lastinput!=null){
					this._moveto(this.prev.lastinput);;
				}
			}
		}
		
	
	},
	_moveright : function(e){
		//if((e.ridx == (this.rows.length - 1)) && (e.cidx == (this.rows[e.ridx].tableCells.length - 1)))return;
		var targetinput = null;
		var finded = false;
		for(var r=e.ridx; r<this.rows.length; r++){
			var startcol = 0;
			if(r == e.ridx){
				startcol = e.cidx + 1;
			}
			for (var c = startcol; c< this.rows[r].tableCells.length; c++){
				if (null!=(targetinput = this._getinput(r,c))){
					this._moveto(targetinput);
					finded = true;
					break;
				}
			}
			if(finded){
				break;
			}
		}
		if(null==targetinput){
			if(this.next!=null){
				if(this.next.firstinput!=null){
					this._moveto(this.next.firstinput);;
				}
			}
		}
	},
	_moveleft : function(e){
		//if(e.ridx == 0 && e.cdix == 0) return;
		var targetinput = null;
		var finded = false;
		for(var r=e.ridx; r>=0; r--){
			var startcol = this.rows[r].tableCells.length-1;
			if(r == e.ridx){
				startcol = e.cidx - 1;
			}
			for (var c = startcol; c>=0; c--){
				if (null!=(targetinput = this._getinput(r,c))){
					this._moveto(targetinput);
					finded = true;
					break;
				}
			}
			if(finded){
				break;
			}
		}
		if(null==targetinput){
			if(this.prev!=null){
				if(this.prev.lastinput!=null){
					this._moveto(this.prev.lastinput);;
				}
			}
		}
	},
	
	_resetFiltersAndSorts : function(){
		this.columns.each(
			function(col,idx){
				if(col.filterType){
					col.filterValues = {}
					var lastTxt = null;
					this.rows.each(function(r){
							var txt = null;
						
							if (r.tableCells[idx]){
								txt = $(r.tableCells[idx]).getText();
								lastTxt = txt;
							}else{
								txt = lastTxt;
							}
							
							if(undefined == col.filterValues[txt]){
								col.filterValues[txt] = [];
							}
							
							col.filterValues[txt].push(r);
						}					
					);
					col.filter.update("");
					var clearer = new Element("div").addClassName("etable_fiter_clear").update("Очистить");
					Event.observe(clearer,"click",(function(){
						 this.unfilter();
					//	 e.filter.toggle();
					}).bind(this));
					 col.filter.insert(clearer);
					for (var fv in col.filterValues) {
						
						link = new Element("div").update(fv).addClassName("etable_filter_element");
						Event.observe(link,"click",function(){ 
								col.etable.filter(col,this.getText());
							});
						col.filter.insert(link);
					}
					
				}
				var prev = null;
				var sorted = this.rows.sortBy(
					function(r){
						var txt = "";
						var _p = false;
						if (r.tableCells[idx]){
							txt = r.tableCells[idx].getText();
							prev = txt;
						}else{
							txt = r.sortKey;
							_p = true;
						}
						var res = txt;
						var num = false;
						
						if(col.columnDataType == this.DATATYPES.NUMBER){
							ntx = "";
							if (txt) ntx = txt.replace(/\s+/,'');
							if(!isNaN(ntx)){
								res =  Number(ntx);
								if(_p)res = res+0.0001;
								num = true;
							}
						}
						// if(_p && !num){
							// res +="_a";
						// }
						if(num){
							res = Number(r.sortKey) + res;
						}else{
							res = r.sortKey + res;
						}
						return res;
					}
				,this)
				sorted.each(
					function(r,idx2){
						r.orders[idx] = idx2;
					}
				);
				
			},this
		)
	},
    sort : function(column){
		this.body.update("");
		var oldorder = column.sortOrder;
		var neworder = oldorder == 1 ? 2 : 1;
		this.columns.each(function(e){
			e.sortOrder = 0;
			e.setAttr("sortorder",0);
			});
		var x = this.currentRows.sortBy(function(r){ 			
				return r.orders[column.columnIndex];
			},this);
		if (2 == neworder){
			x = x.reverse();
		}
		column.sortOrder = neworder;
		column.setAttr("sortorder",neworder);
		x.each(function(r){this.body.insert(r);},this);
		
	},
	filter : function(column,value){
		this.body.update("");
		this.columns.each(function(e){e.sortOrder = 0;});
		column.setAttr("etable_filtered",1);
		this.currentRows = this.currentRows.intersect(column.filterValues[value]);
		this.currentRows.each(function(r){
			this.body.insert(r);
			},this);
		this._closeFilters();
	},
	unfilter : function(column,value){
		this.body.update("");
		this.currentRows = this.rows;
		this.currentRows.each(function(r){
			this.body.insert(r);
			},this);
		this.columns.each(function(c){c.setAttr("etable_filtered",0);});
		this._closeFilters();
	},
	_closeFilters : function(exc){
		this.columns.each(
			function(c){
				if(c!=exc){
						if(c.filter){
						c.filter.hide();
						}
					}
				});
	},
	evaluate : function(){
		for (var l in this.formuls){
			this.formuls[l].each(
				function(cell){
					var val = this.evaluateFormula(cell);
					cell.afterformula(cell, val);
				},this);
		}
		this._resetFiltersAndSorts();
	},
	evaluateFormula : function(cell){
		var val = eval(cell.parsedFormula);
		cell.update(val);
		return val;
	},
	_parse : function(r,c,formula){
		formula = formula.replace(/\$(\w+)/g,"comdiv.ui.Etable._getByElement('"+this.id+"',"+r+","+c+",$$('$1'))");
		formula = formula.replace(/ACELL\(([\d\-]+),([\d\-]+)\)/g,"comdiv.ui.Etable._getByAbsolute('"+this.id+"',"+r+","+c+",$1,$2)");
		formula = formula.replace(/RCELL\(([\d\-]+),([\d\-]+)\)/g,"comdiv.ui.Etable._getByRelative('"+this.id+"',"+r+","+c+",$1,$2)");
		formula = formula.replace(/SUM\(UP\)/g,"comdiv.ui.Etable._sumUp('"+this.id+"',"+r+","+c+",0)");
		formula = formula.replace(/SUM\(LEFT\)/g,"comdiv.ui.Etable._sumLeft('"+this.id+"',"+r+","+c+",0)");
		formula = formula.replace(/SUM\(RIGHT\)/g,"comdiv.ui.Etable._sumRight('"+this.id+"',"+r+","+c+",0)");
		formula = formula.replace(/SUM\(DOWN\)/g,"comdiv.ui.Etable._sumDown('"+this.id+"',"+r+","+c+",0)");
		formula = formula.replace(/SUM\(UP,(\d+)\)/g,"comdiv.ui.Etable._sumUp('"+this.id+"',"+r+","+c+",$1)");
		formula = formula.replace(/SUM\(LEFT,(\d+)\)/g,"comdiv.ui.Etable._sumLeft('"+this.id+"',"+r+","+c+",$1)");
		formula = formula.replace(/SUM\(RIGHT,(\d+)\)/g,"comdiv.ui.Etable._sumRight('"+this.id+"',"+r+","+c+",$1)");
		formula = formula.replace(/SUM\(DOWN,(\d+)\)/g,"comdiv.ui.Etable._sumDown('"+this.id+"',"+r+","+c+",$1)");
		return formula;
	},
	_getcell : function(r,c){
		if(this.rows.length <= r) return null;
		var r = this.rows[r];
		if (r.tableCells.length <= c) return null;
		return r.tableCells[c];
	}
	
});


/////////////////////////   FORMULA EVALUATION HELPER METHODS (STATIC) ///////////////////////////////////////////////////////////////////
Object.extend(comdiv.ui.Etable,{
	_formatNumber : function(number){
		number = number.toString().replace(/\s*/g,'').replace(/,/g,'.');
		number = Number(number).toFixed(2);
		
		var dec = number.match(/\.\d+/);
		if(dec && dec.length > 0) {
			dec = dec[0];
		}else{
			dec = "";
		}
		dec = dec.replace(/0+$/,'');
		if("."==dec)dec="";
		
		number = number.replace(/\.\d+/,"")
		if(number.length >= 3){
			var newnumber = "";
			var c = 0;
			for(var i = number.length - 1; i>=0;i--){
				c++;
				if(c==4){
					newnumber = " "+newnumber;
					c = 0;
				}
				newnumber = number[i]+newnumber;
			}
			number = newnumber;
		}
		number = number+dec;
		return number;
	},
	_getByElement : function(id,r,c,e){
		var etable = $(id).etable;
		e = $(e);
		if(null==e){
			return "";
		}
		var value = "";
		if(null!=e.input){
			value = e.input.value;
		}else{
			value = e.getText();
		}
		var result = value;
		if (!(etable.columns[Number(c)]) || (etable.columns[Number(c)].columnDataType == "number")){
			result = Number(value.replace(/\s*/g,'').replace(/,/g,'.'));
			
		}
		return result;
	},
	_getByAbsolute : function(id,r,c,ra,ca){
		var etable = $(id).etable;
		var element = etable.rows[Number(ra)].tableCells[Number(ca)];
		return this._getByElement(id,r,c,element);
	},
	
	
	
	_getByRelative : function(id,r,c,rr,cr){
		var etable = $(id).etable;
		r = Number(r);
		c = Number(c);
		rr = Number(rr);
		cr  = Number(cr);
		var element = etable.rows[r + rr].tableCells[c + cr];
		return this._getByElement(id,r,c,element);
	},
	_sumUp : function(id,r,c,start){
		var etable = $(id).etable;
		r = Number(r);
		c = Number(c);
		var result = 0;
		for(var i = start; i < r ; i++){
			var val = comdiv.ui.Etable._getByAbsolute(id,r,c,i,c);
			if(!isNaN(val)){
				result += val;
			}
		}
		return this._formatNumber(result);
	},
	_sumLeft : function(id,r,c,start){
		var etable = $(id).etable;
		r = Number(r);
		c = Number(c);
		var result = 0;
		for(var i = start; i < c; i++){
			var val = comdiv.ui.Etable._getByAbsolute(id,r,c,r,i);
			if(!isNaN(val)){
				result += val;
			}
		}
		return this._formatNumber(result);
	},
	
	_sumRight : function(id,r,c,end){
		var etable = $(id).etable;
		r = Number(r);
		c = Number(c);
		var result = 0;
		for(var i = c+1; i <= end; i++){
			var val = comdiv.ui.Etable._getByAbsolute(id,r,c,r,i);
			if(!isNaN(val)){
				result += val;
			}
		}
		return this._formatNumber(result);
	},
	_sumDown : function(id,r,c,end){
		var etable = $(id).etable;
		r = Number(r);
		c = Number(c);
		var result = 0;
		for(var i = r+1; i <= end; i++){
			var val = comdiv.ui.Etable._getByAbsolute(id,r,c,i,c);
			if(!isNaN(val)){
				result += val;
			}
		}
		return this._formatNumber(result);
	}
});


