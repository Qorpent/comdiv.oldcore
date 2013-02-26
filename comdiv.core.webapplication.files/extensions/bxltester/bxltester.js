_result = (function(){
	var result = {
		call : function(type,target,oncomplete){
			var t = target || this.parameters.resultid; 
			var self=  this;
			var imports = "";
			if(type!="save"){
				$$(self.parameters.importcss).each(function(e){
					if(e.checked){
						imports += e.value+",";
					}
				});
			}
			var cmd = type;
			if (cmd != "save"){
				cmd = "compile";
			}
			Ajax.from("bxltester",cmd)
				.post()
				.params({
					code : $(self.parameters.codeid).value,
					name : $(self.parameters.filenameid).value,
					method : type,
					xpath : $(self.parameters.xpathid).value,
					imports : imports
				})
				.after(function(req){
					$(t).value = req.responseText;
					$(t).removeClassName('error');
					if (oncomplete){
						oncomplete(req);
					}
				})
				.error(function(req){
					$(t).value = req.responseText;
					$(t).addClassName('error');
				})
				.call();
		},
		parse : function(){
			this.call('parse');
		},
		smart : function(){
			this.call('smart');
		},
		themas : function(){
			this.call('themas');
		},
		save : function(){
			
			var self = this;
			if($(this.parameters.filenameid).value){
				if($(this.parameters.codeid).value || confirm("Пустой код уничтожит файл, вы уверены в удалении??")){
					this.call('save',this.parameters.saveresultid, function(){ self.loadFileList();});
					$(this.parameters.codeid).removeClassName('changed');
					this.pendingchangesforsave = false;
				}
			}
			
			$(this.parameters.saveresultid).update('Файл сохранен');
			$(this.parameters.saveresultid).removeClassName('changed');
			
			
		},

		stopauto : function(){
			this.useauto = false;
			this.autocounter = 0;

		},
		
		stopautosave : function(){
			this.useautosave = false;
			this.autosavecounter = 0;
		},

		useautosave : false,
		pendingchangesforsave : false,
		autosavestarted : false,
		autosavecounter : 0,
		
		useauto : false,
		autotype : '',
		autocounter : 0,
		pendingchanges : false,
		started : false,

		_autocheck : function(){
			if(!this.useauto) return;
			var self = this;
			this.started = true;
			if(this.autocounter<=0 && this.pendingchanges){
				this.started = false;
				this.autocounter = 0;
				this.pendingchanges = false;
				this.call(this.autotype);
				return;
			}
			if(this.autocounter<=0 && !this.pendingchanges){
				this.started = false;
				this.autocounter = 0;
				return;
			}
			this.autocounter = this.autocounter - 200;
			window.setTimeout(function(){
				self._autocheck();
			},200);
		},
		
		_autosave : function(){
			if(!this.useautosave) return;
			var self = this;
			this.autosavestarted = true;
			if(this.autosavecounter<=0 && this.pendingchangesforsave){
				this.autosavestarted = false;
				this.autosavecounter = 0;
				this.pendingchangesforsave = false;
				$(this.parameters.codeid).removeClassName('changed');
				this.save();
				return;
			}
			if(this.autosavecounter<=0 && !this.pendingchangesforsave){
				this.autosavestarted = false;
				this.autosavecounter = 0;
				return;
			}
			this.autosavecounter = this.autosavecounter - 3000;
			window.setTimeout(function(){
				self._autosave();
			},3000);
		},
		
		
		loadfile : function(code){
			var self = this;
			Ajax.from("bxltester","load")
				.param("name",code)
				.sync()
				.after(function(req){
					$(self.parameters.codeid).value = req.responseText;
				})
				.call();
			$(self.parameters.filenameid).value = code;
			self.startauto();
		},
		
		loadFileList : function(){
			var self = this;
			Ajax.from("bxltester","files")
				.into(self.parameters.filelistid)
				.sync()
				.after(function(){
					$$(self.parameters.importcss).each(function(e){
						Event.observe(e,'click',function(){
							self.parameters.profile.data[e.value] = e.checked;
							self.startauto();
						});
						e.checked = false;
						if (self.parameters.profile.data[e.value]){
							e.checked = true;
						}
					});
					
					$$(self.parameters.importlinkcss).each(function(e){
						Event.observe(e,'click',function(){
							try{
								if((!self.pendingchangesforsave) || confirm("Вы уверены что не надо сохранить текущую работу")){
									self.loadfile(e.getAttribute("value"));
								}
							}catch(e){
								$(self.parameters.codeid).value = e;
								
							}
							return false;
						});
					});
				})
				.call();
		},
		

		startauto : function(){
			var self = this;
			$$(self.parameters.autocss).each( function(e){
				if(e.checked){
					 self.useauto = true;
					 self.autotype = e.value;
					 self.pendingchanges = true;
					 self.autocounter = 0;
					 self._autocheck();
				}
			});
		},
		
		startautosave : function(){
			var self = this;
			var e = $(self.parameters.autosaveid);
			if(e.checked){
				 self.useautosave = true;
				 self.pendingchangesforsave = true;
				 self.autosavecounter = 0;
				 self._autosave();
			}
		},

		parameters : {
			codeid : 'bxl_code',
			filenameid : 'bxl_file_name',
			resultid :'bxl_result',
			autocss :'.bxl_auto',
			processbuttoncss : '.bxl_button',
			importcss :'.bxl_importfile',
			importlinkcss :'.bxl_importfilelink',
			autosaveid : 'bxl_autosave',
			savecss : '.bxl_save',
			saveresultid :'bxl_saveresult',
			filelistid : 'bxl_filelist',
			xpathid : 'bxl_xpath',
			profile : null
		},
		
		prepare : function(options){
			Object.extend(this.parameters, options || {});
			if(!(this.parameters.profile)){
				this.parameters.profile = zeta.profile.load("bxltester", { 
						autoxml : false, 
						autobxl : false, 
						autothemas : false,
						autosave : false
				});
			}
			
			var self = this;
			
			$$(this.parameters.autocss).each(function(e){
				Event.observe(e,'click',function(){
					self.stopauto();
					var c = e.checked;
					$$(self.parameters.autocss).each(function(e2){e2.checked = false;});
					e.checked = c;
					var data = self.parameters.profile.data;
					data.autoxml = false;
					data.autobxl = false;
					data.autothemas = false;
					if(e.value == "parse" && e.checked){
						data.autoxml = true;
					}else if (e.value == "smart" && e.cheched){
						data.autobxl = true;
					}else if (e.value == "themas" && e.checked){
						data.autothemas = true;
					}
					self.startauto();
				});
				e.checked = false;
			});
			/// проверено
			$$(this.parameters.processbuttoncss).each(function(e){
				var method = e.getAttribute('method');
				Event.observe(e,'click',function(){
					self.call(method);					
				});
				e.checked = false;
			});
			
			
			Event.observe($(this.parameters.xpathid),"change",function(){
					self.startauto();
			});
			
			Event.observe($(this.parameters.autosaveid),'click',function(){
				self.parameters.profile.data.autosave = $(self.parameters.autosaveid).checked;
				self.stopautosave();
				self.startautosave();
				
			});
			
			//проверено
			
			
			$$(this.parameters.savecss).each(function(e){
				Event.observe(e,'click',function(){
					self.save();
				});
				e.checked = false;
			});
			
			
			$(this.parameters.autosaveid).checked= false;
			
			Event.observe($(this.parameters.codeid),'keydown', (function(){
				self.pendingchangesforsave = true;
				
				if(self.useauto ){
					self.autocounter = 300;
					self.pendingchanges = true;
					if(!self.started){
						self._autocheck();
					}
				}
				if($(self.parameters.filenameid).value){
					$(self.parameters.saveresultid).update('Файл не сохранен');
					$(self.parameters.saveresultid).addClassName('changed');
					$(self.parameters.codeid).addClassName('changed');
					
					if(self.useautosave ){
						self.autosavecounter = 3000;
						if(!self.autosavestarted){
							self._autosave();
						}
					}
				}
			}));
			//проверено
			
			$(this.parameters.filenameid).value = "";
			this.loadFileList();
			var profile = this.parameters.profile.data;
			var autofound = false;
			
			$$(this.parameters.autocss).each(
					function(e){
						if(autofound) return;
						if (
							(e.value == "parse" && profile.autoxml)
							||
							(e.value == "smart" && profile.autobxl)
							||
							(e.value == "themas" && profile.autothemas)
							) {
							autofound = true;
							e.checked = true;
						}
					}
			);
			
			
			
			if(profile.autosave){
				$(this.parameters.autosaveid).checked = true;
				this.startautosave();
			}
			
			//проверено
			if (Prototype.Browser.IE) {
				$(this.parameters.codeid).style.width = "250px";
				$(this.parameters.codeid).style.height = "500px";
				$(this.parameters.resultid).style.width = "250px";
				$(this.parameters.resultid).style.height = "500px";
			}else{
				$(this.parameters.codeid).style.width = (document.viewport.getWidth() * 35 / 100 )+"px";
				$(this.parameters.resultid).style.width = (document.viewport.getWidth() * 35 / 100 )+"px";
			//	$(this.parameters.codeid).style.width = (document.viewport.getWidth() / 2 - 30) + 'px';
			//	$(this.parameters.resultid).style.width = (document.viewport.getWidth() / 2 - 30) + 'px';
				$(this.parameters.resultid).style.height = (window.innerHeight * 75 / 100 )+"px";
				$(this.parameters.codeid).style.height = (window.innerHeight * 75 / 100 )+"px";
			}
			this.parameters.profile.autosave();
			this.startauto();
			zeta.toolbar.show('dev');
			zeta.toolbar.updateTitle("Редактор BXL", "Безопасная среда редактирования и отладки BXL-файлов");
		},
		
		initialize : function(options){
			Event.observe(window,'load',(function(){
				this.prepare(options);
			}).bind(this));
			return this;
		}
	};
	return result;
});
var zeta = zeta ? zeta : {};
zeta.bxl = zeta.bxl ? zeta.bxl : {};
Object.extend(zeta.bxl,_result());
zeta.bxl.initialize();