zeta = zeta ? zeta : {};
zeta.files = zeta.files ? zeta.files : {};
Object.extend(zeta.files,{
	init : function(){
		this.searchinput = $('file-manager-browser-search');
		
		zeta.toolbar.updateTitle('Файловый менеджер','Управление файлами приложения');
		
		Event.observe($('file-manager-browser-search-button'),'click',(function(){
			this.search();
		}).bind(this));
		
		Event.observe(this.searchinput,'keydown',(function(event){
			if(event.keyCode == 13) {
				this.search();
			}
		}).bind(this));
		
		$$('.file_manager_editor').each(function(e){
			this.makeEditor(e);
		}.bind(this));
		
		zeta.toolbar.show('files');
		
		this.searchinput.focus();
	},
	makeEditor : function(e){
		this.editor = e;
		var self = this;
		var tabzone = new Element("div",{"class":"tabzone"});
		var filezone = new Element("div",{"class":"filezone"});
		
		
		e.appendChild(tabzone);
		e.appendChild(filezone);
		e.tabzone = tabzone;
		e.filezone = filezone;
		e.tabzone.inner = new Element("div",{"class":"inner"});
		e.tabzone.appendChild(e.tabzone.inner);
		e.tabzone.appendChild(new Element("div",{"style":"clear:both;"}));
		
		Object.extend(e,{
			manager : self,
			getCurrentText : function(){
				return $('ta_'+this.activefile).value;
			},
			open : function(f){
				
				var ta = new Element("textarea");
				var tt = new Element("div");
				ta.setAttribute("spellcheck","false");
				ta.setAttribute("cols",40);
				ta.setAttribute("wrap","off");
				tt.addClassName("tab");
				ta.addClassName("area");
				ta.setAttribute("id","ta_"+f);
				tt.setAttribute("id","tt_"+f);
				tt.update(f.replace('extensions/','X/').replace('extensionslib/','L/'));
				tt.file=f;
				var closer = new Element("div",{"class":"tabcloser"});
				closer.update("&nbsp;");
				tt.appendChild(closer);
				this.filezone.appendChild(ta);
				this.tabzone.inner.appendChild(tt);
				Event.observe(tt,'click',function(){
					this.activate(f);
				}.bind(this));
				Event.observe(ta,'keydown',function(e){
					if(e.keyCode!=16 && e.keyCode!=17 && e.keyCode!=18 && e.keyCode!=27 && e.keyCode!=91){
						this.changed(f);
					}
				}.bind(this));
				Event.observe(closer,'click',function(event){
					this.close(f);
					event.stop();
				}.bind(this));
				this.manager.open(f,ta);
				this.activate(f);
				zeta.ui.textarea.addlinenumbers(ta);
			},
			changed : function(f){
				$('ta_'+f).addClassName('changed');
				$('tt_'+f).addClassName('changed');
			},
			deletefile : function(file){
				if(confirm("Вы уверены?")){
					Ajax.from("filemanager/delete").param("filename",file)
						.after(function(){
							if($('tr_'+file)){
								$('tr_'+file).remove();
							}
						}).call();
				}
			},
			save : function(file, code, templateobj, edittemplate){
				if(code){
					templateobj = templateobj || {};
					if(edittemplate){
						json = Ajax.from("filemanager/gettemplatejson").param("code",code).eval();
						currentjson = Ajax.from("filemanager/getcurrentjson").param("file",file).eval();
						templateobj = Object.extend(json,templateobj);
						templateobj = Object.extend(templateobj,currentjson);
					}
				}
				comdiv.modal.editobject("Редактировать шаблон "+code,templateobj,function(obj){
					file = file || this.activefile;
					if (!file){
						zeta.sys.shortResult.update("Сохранение", "Файл не указан");
						return;
					}
					var f = file;
					var ta = $('ta_'+f);
					if(!code){
						if(!ta.hasClassName('changed')){
							zeta.sys.shortResult.update("Сохранение", "Текущий файл не изменен");
							return;
						}
					}
					content = "";
					if(ta)content = ta.value;
					q = Ajax.from("filemanager","set").param("filename",f);
					if(code){
						objparam = {};
						for (i in obj){
							t = typeof(obj[i]);
							v = obj[i];
							if(t=="string"||t=="boolean"||t=="number"||v==null){
								objparam[i] = v;
							}
						}
						q.param("code",code);
						q.param("json",Object.toJSON(objparam));
					}else{
						q.param("content",content)
						
					}
					q.after(function(){
						if(code){
							this.reload(file);
						}
						zeta.sys.shortResult.update("Сохранение", "Файл успешно сохранен");
						if($('ta_'+file)){
							ta.removeClassName('changed');
						}
						if($('tt_'+f)){
							$('tt_'+f).removeClassName('changed');
						}
					}.bind(this))
					.call();
				}.bind(this), !edittemplate);
			},
			reload : function(){
				if (!this.activefile){
					zeta.sys.shortResult.update("Перезагрузка", "Нет открытого файла");
					return;
				}
				var f = this.activefile;
				var ta = $('ta_'+f);
				Ajax.from("filemanager","get").param("filename",f)
					.after(function(r){
						zeta.sys.shortResult.update("Перезагрузка", "Файл успешно перезагружен");
						ta.removeClassName('changed');
						$('tt_'+f).removeClassName('changed');
						ta.value = r.responseText;
					}).call();
			},
			close : function(f){
				f = f || this.activefile;
				var ta = $('ta_'+f);
				if(ta.hasClassName('changed')){
					if(!confirm("Изменения не сохранены, вы уверены, что хотите закрыть этот файл:\r\n"+f)){
						return;
					}
				}
				ta.remove();
				if(this.activefile == f){
					zeta.toolbar.updateTitle(null,null,'  ');
					var finded = false;
					var newactive = "";
					Selector.findChildElements(this.tabzone,['.tab',]).each(function(e){
						if(e.file==f){
							finded = true;
						}else{
							if(!finded || !newactive){
								newactive = e.file;
							}
						}
					});
					if(newactive){
						this.activate(newactive);
					}else{
						this.activefile = "";
					}
				}
				$('tt_'+f).remove();
			},
			closeall : function(){
				tabs = $A(Selector.findChildElements(this.tabzone,['.tab',]));
				tabs.each(function(e){
					this.close(e.file);
				}.bind(this));
			},
			reloadall : function(){
				tabs = $A(Selector.findChildElements(this.tabzone,['.tab',]));
				tabs.each(function(e){
					this.reload(e.file);
				}.bind(this));
			},
			saveall : function(){
				Selector.findChildElements(this.tabzone,['.tab',]).each(function(e){
						if(e.hasClassName('changed')){
							this.save(e.file);
						}
					}.bind(this));
			},
			create : function(){
				comdiv.modal.prompt("Укажите имя файла", function(name){
					Ajax.from("filemanager","set")
						.param("filename",name)
						.param("content","")
						.sync()
						.call();
					this.activate(name);
				}.bind(this));
			},
			activate : function(f){
				var was = false;
				Selector.findChildElements(this.filezone,['.area',]).each(function(e){
					e.hide();
					if(e.getAttribute('id') == 'ta_'+f){
						was = true;
						e.show();
					}
				});
				if(!was){
					this.open(f);
					return;
				}
				Selector.findChildElements(this.tabzone,['.tab',]).each(function(e){
					e.removeClassName('selected');
					if(e.getAttribute('id') == 'tt_'+f){
						e.addClassName('selected');
					}
				});
				this.activefile = f;
				zeta.toolbar.updateTitle(null,null,"- "+f);
				this.reevalFileZone();
			},
			reevalFileZone : function(){
				var totalheight = this.getHeight();
				var tabheight = this.tabzone.getHeight();
				this.filezone.style.height = (totalheight - tabheight - 15)+"px";
				this.filezone.style.top = (tabheight + 3)+"px";
			}
		});
		
		e.reevalFileZone();
	},
	
	search : function(){
		this.searchinput.removeClassName('invalid');
		var query = this.searchinput.value;
		$$('.file_filter').each(function(e){
			query += e.getfilter();
		});
		if(!query){
			this.searchinput.addClassName('invalid');
			zeta.sys.shortResult.update('Поиск файлов','Требуется указать маску поиска');
			return;
		}
		Ajax
			.from('filemanager','search')
			.param('query',query)
			.param('_view','/filemanager/browser_files')
			.update($('file-manager-browser-files'));
	},
	
	open : function(file, e) {
		Ajax
			.from('filemanager','get')
			.param('filename',file)
			.after(function(r){
				e.value = r.responseText;
			})
			.call();
	},
	
	executescript : function(code, parameters, editparams){
		if(!code){
			if(this.editor.activefile && this.editor.activefile.match(/\.fs\.script/) ){
				code = this.editor.activefile;
			}
		}
		comdiv.modal.prompt(
			"Укажите код скрипта", function(scriptcode){
				defparams = Ajax.from("filemanager/getscriptparams").param("code",scriptcode).eval();
				paramseters = Object.extend(defparams,parameters);
				comdiv.modal.editobject(
					"Настройте вызов скрипта",
					paramseters,
					function(params){
						params = Object.extend(params||{},{code:scriptcode});
						new comdiv.modal.dialog({
							title : "Результат выполнения скрипта "+scriptcode,
							action : "filemanager/executescript",
							parameters : params,
						});
					},!editparams);
				}
			, code, false,false, !!code
		);
	},
	
	filters : {},
	
	__initfilters : function(){
		target= $('fmfilters');
		for (f in this.filters){
			filter = this.filters[f];
			e = new Element("div",{"class":"file_filter", value : filter.filter, title : filter.title});
			e.value = " "+filter.filter;
			cb = new Element("input",{type:"checkbox", code : filter.code});
			cb.code = filter.code;
			e.box = cb;
			e.appendChild(cb);
			if(localStorage.getItem("__fm_checked_"+filter.code)=="true"){
				cb.checked = true;
			}
			Event.observe(cb,"click",function(){
				localStorage.setItem("__fm_checked_"+this.code, this.checked);
			}.bind(cb));
			txt = new Element("span").update(filter.name);
			e.appendChild(txt);
			e.getfilter = function(){
				if(this.box.checked){
					return this.value;
				}else{
					return "";
				}
			
			}.bind(e);
			target.appendChild(e);
		}
	},
});
Event.observe(window,'load',function(){
	zeta.files.init();
	
	if(!zeta.files.filters["nobak"]){
		zeta.files.filters["nobak"] = {
			code : "nobak",
			name : "Без .BAK",
			title : "Исключает из поиска резервные копии файлов",
			filter : "!.bak",
		};
	}
	if(!zeta.files.filters["nolib"]){
		zeta.files.filters["nolib"] = {
			code : "nolib",
			name : "Без xLIB",
			title : "Исключает из поиска файлы библиотеки расширений",
			filter : "!extensionslib",
		};
	}
	
	zeta.files.__initfilters();
	
	Event.observe(document.body,"keydown",function(e){
		if(e.ctrlKey){
			if(e.keyCode == 78){
				Event.stop(e);
				zeta.files.editor.create();
				return false;
			}
			if(e.keyCode == 82){
				Event.stop(e);
				if(e.shiftKey){
				zeta.files.editor.reloadall();
				}else{
					zeta.files.editor.reload();
				}
				return false;
			}
			if(e.keyCode == 83){
				Event.stop(e);
				if(e.shiftKey){
				zeta.files.editor.saveall();
				}else{
					zeta.files.editor.save();
				}
				return false;
			}
			if(e.keyCode == 81){
				Event.stop(e);
				if(e.shiftKey){
				zeta.files.editor.closeall();
				}else{
					zeta.files.editor.close();
				}
				return false;
			}
		}
	});
});

