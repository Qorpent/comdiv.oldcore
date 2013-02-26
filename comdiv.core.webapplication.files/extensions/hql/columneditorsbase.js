var zeta = zeta ? zeta : {};
zeta.hql = zeta.hql ? zeta.hql : {};
zeta.hql.columneditorbase = Class.create({
		initialize : function(){
			this.__reverter = function(e){
				e.item.editmode = false;
				this.__revertvalue(e);
				e.removeClassName("inchangemode");			
			}.bind(this);
			this.__applyer = function(e){
				zeta.tableutils.extendcell(e);
				e.item.editmode = false;
				e.removeClassName("inchangemode");
				if(this.__getadditionalapply){
					addtion = this.__getadditionalapply(e.item, this.__getcellvalue(e));
					zeta.hql.update(
					e.evalAttr("entity"),
					e.evalAttr("entityid"),
					addtion.propname,
					addtion.value
					);
				}
				zeta.hql.update(
					e.evalAttr("entity"),
					e.evalAttr("entityid"),
					e.item.propname,
					this.__getcellvalue(e),
					function(){
						this.__setcellinitialvalue(e);
					}.bind(this)
				);
				if (this.__afterapply){
					this.__afterapply(e.item);
				}
			}.bind(this);
		},
		
		
		
		execute : function(){
			items = $A($$(this.cssquery));
			for(var i=0;i<items.length;i++){
				item = items[i];
				if(!item.__is_hql_preparecheckitemes){
					this.__initializeitem(item);
					this.__initializevalue(item);
					if(item.editable){
						item.cell.revert = this.__reverter;
						item.cell.apply = this.__applyer;
						this.__prepareeditableitem(item);
					}
				}			
			}
		},
		cssquery : "",
		__getcell : function(item){
			return item.up();
		},
		
		__addretescsupport : function(item){
			Event.observe(item,"keydown",func = function(event){
				if(this.editmode && event.keyCode == Event.KEY_RETURN || event.keyCode == Event.KEY_ESC){
					this.cell =  zeta.tableutils.extendcell(this.cell);
					this.editmode = false;
					this.cell.removeClassName("inchangemode");
					Event.stop(event);
					if(event.keyCode == Event.KEY_RETURN){	
						this.cell.apply(this.cell);
					}else if(event.keyCode == Event.KEY_ESC){
						this.cell.revert(this.cell);
					}
					return false;
				}
			}.bind(item));
		
		},
		
		__addonchange : function(item){
			self = this;
			Event.observe(item,"change",func=function(event){
				if(!this.editmode){
					this.editmode = true;
					this.cell.addClassName("inchangemode");
				}		
				if(self.__getitemvalue(this) == this.initial){
					this.editmode = false;
					this.cell.removeClassName("inchangemode");
				}
			}.bind(item));
		},
		
		
		__getcellvalue : function(cell){
			return this.__getitemvalue(cell.item);
		},
		
		__getitemvalue : function(item){
		
		
		},
		__setitemvalue : function(item,value){
		
		
		},
		
		__prepareeditableitem : function(item){
		
		
		},
		
		__setcellinitialvalue : function(cell){
			cell.item.initial = this.__getitemvalue(cell.item);
		},
		
		__initializeitem : function(item){
			item.__is_hql_preparecheckitemes = true;
			item.editable = item.getAttribute("editable") == "True";
			item.propname = item.getAttribute("name");
			item.initial = item.getAttribute("initial");
			item.editmode = false;
			item.cell = this.__getcell(item);
			item.cell.customedit = true;
			item.cell.item = item;
			this.__initializeitemext(item);
		},
		
		__initializeitemext : function(item){
		
		},
		
		__initializevalue : function(item){
			this.__setitemvalue(item,item.initial);
		},
		
		__revertvalue : function(cell){
			this.__setitemvalue(cell.item, cell.item.initial);
		},
	
		
	});