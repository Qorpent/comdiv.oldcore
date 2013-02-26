_result = (function () {
    var result = {		
		addrecord: function(code, name) {
			zeta.hql.execute("create " + $('hqlresulttable').getAttribute('entity') + ", CODE='" + code + "', NAME='" + name + "'");
		},
		
        execute: function (query, system, view) {
			if(query){
				query = this.preprocessquery(query);
			}
		
			if(query){
				$(this.parameters.query).value = query;
			}else{
				query = $(this.parameters.query).value;
			}
			view = view || "";
			if(view){
				if($('hqlview')){
					$('hqlview').value = view;
				}
			}else{
				if($('hqlview')){
					view = $('hqlview').value;
				}
			}
			
			if(!query){
				return;
			}
			if(!system){
				system = "Default"
				if (zeta.sql){
					system = zeta.sql.getCurrentSystem();
				}
			}
			
            var parameters = {
                query: query,
				view :view,
				system : system,
            };
            Ajax
                .from("hql", "execute")
                .params(parameters)
				.after(function(req,waserror){
					if(!waserror){
						zeta.sys.log.append("Запрос '"+$(zeta.hql.parameters.query).value+"' на системе '"+system+"' выпполнен успешно'");
						$(this.parameters.result).scrollLeft = 0;
						$(this.parameters.result).scrollTop = 0;
					}
				}.bind(this))
				.error(function(){
					zeta.sys.log.append("Запрос '"+$(zeta.hql.parameters.query).value+"' на системе '"+system+"' выпполнен с ошибкой'");
				})
                .update(this.parameters.result);
        },
		
		
		
		revertall : function(){
			changes = $A($$(".inchangemode"));
			for (i =0;i<changes.length;i++){
				change = changes[i];
				if(change.revert){
					change.revert(change);
				}
			}
		},
		
		applyall : function(){
			changes = $A($$(".inchangemode"));
			for (i =0;i<changes.length;i++){
				change = changes[i];
				if(change.apply){
					change.apply(change);
				}
			}
		},
		
		preprocessquery : function(query){
			query = query.replace(/\{1\}/g,$('template1').value);			
			query = query.replace(/\{2\}/g,$('template2').value);
			query = query.replace(/\{3\}/g,$('template3').value);
			query = query.replace(/~/g,"'");
			return query;
		},
		
		del : function(type,id,system, callback){
			if(confirm("Вы уверены в удалении этого элемента?")){
			system = system || zeta.sql.getCurrentSystem();
			Ajax.from("hql/delete").params({type:type,id:id,system:system})
				.after(function(){
					if(callback){
						callback();
					}
				})
				.error(function(r){
					comdiv.modal.alert(r.responseText);
				})
				.call();
			}
		},

		__menu : null,
		
		onmenu : function(e, id, entity){
			$$(".protoMenu").each(function(e){e.remove();});
			this.__menu  = new Proto.Menu({onPopulate:(function(e,menu){
			this.__populateMenu(e,menu,id,entity);}).bind(this)});
			this.__menu.show(e);
		},
		
		__onmenuhandlers : [],
		
		__populateMenu : function(e, menu,id,entity){
			menu.list.addItem({name:"Удалить",callback : function(){
				tr = $('hql_tr_'+entity+"_"+id);
				table = tr.up("table");
				sys = table.getAttribute("system");
				zeta.hql.del(entity,id,sys,function(){
					tr.remove();
				});
			}});
			this.__onmenuhandlers.each(function(f){f(e,menu,id,entity);});
		},
		
		registerOnMenuHandler : function (func) {
			this.__onmenuhandlers.push(func);
		},
		
		addMenu : function (type, name, callback){
			zeta.hql.registerOnMenuHandler(function(e,menu,id,entity){
				if(!type || entity==type){
					menu.list.addItem({
						name : name,
						callback : function(){callback(e,menu,id,entity);}
					});
				}					
			});
		},

        parameters: {
            query: "hql_query",
            result: "hql_result",
        },

        prepare: function (options) {
            Object.extend(this.parameters, options || {});
            if (!(this.parameters.profile)) {
                this.parameters.profile = zeta.profile.load("hql", { }, "local");
            }
			
			if(this.parameters.query){
				window.setTimeout(function(){
					zeta.ui.textarea.addlinenumbers(this.parameters.query);
				}.bind(this),200);
			}

            this.parameters.profile.autosave();
			if(zeta.application.controller=="hql"){
				//zeta.toolbar.show('bd');
				zeta.toolbar.updateTitle('Консоль HQL', 'Прямой вызов запросов HQL в основной конфигурации');
				
				Event.observe(window,"keydown",function(e){
					if(e.shiftKey && e.keyCode==116){
						Event.stop(e);
						zeta.hql.execute();
						return false;
					}
					if(e.shiftKey && e.altKey && e.keyCode==13){
						Event.stop(e);
						zeta.hql.applyall();
						return false;
					}
					if(e.shiftKey && e.altKey && e.keyCode==8){
						Event.stop(e);
						zeta.hql.revertall();
						return false;
					}
				});
			}
        },
		
		update : function(entity, entityid, prop, value, callback,errorcallback){
			
			system = "Default"
			if (zeta.sql){
				system = zeta.sql.getCurrentSystem();
			}
			var params = {
						entity : entity, id : entityid, prop:prop, value : value, system : system
					};
			Ajax
				.from("hql/update")
				.params(params)
				.sync()
				.after(function(e,err){
					
					if(!err){
						zeta.sys.log.append("Apply complete "+Object.toJSON(params));
						if(callback){
							callback(entity,entityid,prop,value);
						}
					}
				})
				.error(function(r){
					zeta.sys.shortResult.update("error",r.responseText);
					zeta.sys.log.append("Error on apply "+Object.toJSON(params));
					if(errorcallback){
						errorcallback(entity,entityid,prop,value,r.responseText);
					}
				})
				.call();
			
			
			
		},
		
		applycell : function(cell,value,text){
			var td = zeta.tableutils.extendcell($(cell));
			if( value == td.oldvalue )return;
			entity = td.evalAttr('entity');
			entityid = td.evalAttr('entityid');
			prop = td.evalAttr('code');
			zeta.hql.update(entity,entityid,prop,value);
			td.oldvalue = value;
			if(td.realupdatetarget){
				td.realupdatetarget.update(text);
			}else{
				td.update(text);
			}
		},
		

		__processcell : function(form,value){
					var td = zeta.tableutils.extendcell($(form).up('td'));
					if( value == td.oldvalue )return;
					entity = td.evalAttr('entity');
					entityid = td.evalAttr('entityid');
					prop = td.evalAttr('code');
					zeta.hql.update(entity,entityid,prop,value);
					return value;
				},

        initialize: function (options) {
            Event.observe(window, 'load', (function () {
                this.prepare(options);
            }).bind(this));
			
            return this;
        }
    };
    return result;
});
var zeta = zeta ? zeta : {};
zeta.hql = zeta.hql ? zeta.hql : {};
Object.extend(zeta.hql,_result());
zeta.hql.initialize();