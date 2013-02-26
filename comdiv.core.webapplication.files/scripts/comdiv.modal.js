var comdiv = Object.isUndefined(comdiv) ? {} : comdiv;
comdiv.modal = Object.isUndefined(comdiv.modal) ? {} : comdiv.modal;
comdiv.modal.dialog = Object.isUndefined(comdiv.modal.dialog) ? Class.create() : comdiv.modal.dialog;

Object.extend(comdiv.modal,{
	dialogs : [] ,
	id : 1,
	add : function(dialog){
		this.dialogs.push(dialog);
		dialog.options.level = this.dialogs.length;
	},
	remove : function(dialog){
		this.dialogs = this.dialogs.without(dialog);
	},
	alert : function(message){
		new comdiv.modal.dialog(
			{
				content : "<span style='font-size:14pt'>"+message+"</span>",
				dialogclass : "error",
			}
		);
	},
	message : function(message){
		new comdiv.modal.dialog(
			{
				content : "<span style='font-size:12pt'>"+message+"</span>",
				dialogclass : "message",
			}
		);
	},
	prompt : function(message, reaction){
		var id = 'prompt_' + Math.floor(Math.random() * 99999);
		var element = new Element("input",{id : id, "class" : "promptinput"});
		
		var dialog = new comdiv.modal.dialog(
			{
				title : message,
				contentelement : element,
				afterApply : function(e){
					result = $(id).value;
					if (reaction && result) {
						reaction(result);
					}
				},
				dialogclass : "prompt",
				height : 20,
				width : 300,
				onLoad : function(d){
					$(id).focus();
					
				},
				
			}
		);	
		Event.observe($(id),'keydown',function(e){
			if(e.keyCode == 13){
				dialog.apply();
			}
		});
		
	}
});


Object.extend(comdiv.modal.dialog.prototype, {
	
	_getDefaultOptions : function(){
		return {
			level : 1,
			width : 700,
			height : 500,
			left : -1,
			top : -1,
			title : "",
			action : "",
			parameters : "",
			content : "",
			targetelement : "",
			contentelement :null,
			showoninit : true,
			registerlevel : true,
			doLoad : (this._doLoad).bind(this),
			onLoad : (this._onLoad).bind(this),
			onValidate : (this._onValidate).bind(this),
			onApply : (this._onApply).bind(this),
			afterApply : function(){},
			droponclose : true,
			modal : true,
			dialogclass : "default",
			showclosebutton : true,
			showokbutton :true,
			showcancelbutton : true,
			method : "POST",
		}
	},
	
	options : null,
	
	initialize : function (options) {
		var options = Object.isUndefined(options) ? {} : options;
		this.options = Object.extend(this._getDefaultOptions(), options);
		if(this.options.registerlevel)comdiv.modal.add(this);
		this.id = comdiv.modal.id++;
		this.element = this._constructElement();
		if(this.options.modal){
			this.shader = this._constructShader();
			document.body.appendChild(this.shader);
		}
		document.body.appendChild(this.element);
		this.setPosition();
		if(!this.options.showoninit){
			this.hide();
		}
		if(this.options.doLoad){
			this.options.doLoad(this);
			if(this.options.onLoad){
				this.options.onLoad(this);
			}
		}
		if(!this.options.modal){
			this._windowOnTop();
		}
	},
		
	_doLoad : function(){
		if(this.options.action){
			this.options.parameters = Object.extend(this.options.parameters || {} ,{ajax:1, dialog:this.id});
			if(utils){
				utils.nocall_error = true;
				$$U(this.element.content,this.options.action,this.options.parameters,null,true);
				utils.nocall_error = false;
			}else{
				Ajax
					.from(this.options.action)
					.params(this.options.parameters)
					.method(this.options.method)
					.sync()
					.update(this.element.content);
			}
		}else if(this.options.content){
			this.element.content.update(this.options.content);
		}else if(this.options.targetelement){
			this.element.content.update($(this.options.targetelement).innerHTML.replace(/\^/g,''));			
		}else if(this.options.contentelement){
			this.element.content.appendChild(this.options.contentelement);
		}
	},
	
	_onLoad : function(){
	},
	_onValidate : function(){
		return true;
	},
	_onApply : function(){
		var form = this.element.content.down("form[autosend]");
		if(form){
			var url = form.getAttribute("autosend");
			var parameters = form.serialize();
			$$R(url,parameters,null,true);
		}
	},
	
	
	hide : function(){
		this.element.hide();
		if(this.shader){
			this.shader.hide();
		}
	},
	
	show : function(){
		this.element.show();
		if(this.shader){
			this.shader.show();
		}
	},
	
	
	_constructShader : function(){
		var base = 1000;
		if(this.options.modal){
			base = 3000;
		}
		var z_index = base + this.options.level * 100;
		var result = new Element("div",{
			class:"comdiv_modal_shader", 
			style : "background-color:black;opacity:0.2;position:fixed;left:0px;top:0px;width:2000px;height:2000px;z-index:"+z_index
		});
		
		Event.observe(result,"mousedown",function(event){event.stop();return false;});
		return result;
	},
	
	_constructElement : function(){
		var base = 1000;
		if(this.options.modal){
			base = 3000;
		}
		var z_index = base + this.options.level * 100 + 10;
		var result = new Element("div",{
			class : "comdiv_modal_dialog dialog_"+this.options.dialogclass ,
			style : "position:fixed;z-index:"+z_index,
		});
		
		result.dialog = this;
		result.setAttribute("id", "comdiv_dialog_"+this.id);
		
		var handle = new Element("div",{class : "comdiv_modal_dialog_handle"});
		
		var title = new Element("div",{class : "comdiv_modal_dialog_title"}).update(this.options.title);
		
		var topcloser = new Element("div",{class : "comdiv_modal_dialog_closer"});
		var topcloserbutton = new Element("div",{class:"topbut"}).update("Закрыть");
		Event.observe(topcloserbutton,"click",(function(){this.close()}).bind(this));
		topcloser.appendChild(topcloserbutton);
		topcloser.closebutton = topcloserbutton;
		result.closebutton = topcloserbutton;
		
		
		
		var botcloser = new Element("div",{class : "comdiv_modal_dialog_closer"});
		var botokbutton = new Element("div",{class:"topbut"}).update("&nbsp;&nbsp;ОК&nbsp;&nbsp;");
		var botcloserbutton = new Element("div",{class:"topbut"}).update("Отмена");
		Event.observe(botcloserbutton,"click",(function(){this.close()}).bind(this));
		Event.observe(botokbutton,"click",(function(){this.apply()}).bind(this));
		botcloser.appendChild(botokbutton);
		botcloser.appendChild(botcloserbutton);
		botcloser.okbutton = botokbutton;
		botcloser.cancelbutton = botcloserbutton;
		result.okbutton = botcloser.okbutton;
		result.cancelbutton = botcloser.cancelbutton;
		
		var content = new Element("div",{class:"comdiv_modal_dialog_content"});
		
		result.appendChild(handle);
		handle.appendChild(topcloser);
		handle.appendChild(title);
		result.appendChild(new Element("div",{style:"clear:both;"}));
		result.appendChild(content);
		result.appendChild(botcloser);
		
		result.handle = handle;
		result.header = topcloser;
		result.title = title;
		result.content = content;
		result.footer = botcloser;
		
		if(!this.options.modal){
			Event.observe(handle, "mousedown", (this._windowOnTop).bind(this));
			
		}
		
		new Draggable(result,{handle: result.handle});
		
		if(!this.options.showclosebutton){
			topcloserbutton.hide();
		}
		if(!this.options.showokbutton){
			botokbutton.hide();
		}
		if(!this.options.showcancelbutton){
			botcloserbutton.hide();
		}
		
		return result;
		
	},
	
	_windowOnTop : function(){
		comdiv.modal.dialogs.each(function(d,i){
			d.element.setStyle({zIndex:1100+i*10});
		});
		this.element.setStyle({zIndex:2000});
	},
	
	setPosition : function(){
		this.element.content.setStyle({width:this.options.width+"px",height:this.options.height+"px"});
		this.element.setStyle({width:(this.options.width+4)+"px"});
		if(this.options.left==-1){
			this.options.left = (document.viewport.getWidth() / 2) - this.element.content.getWidth() /2 - 2;
		}
		if(this.options.top==-1){
			this.options.top = (document.viewport.getHeight() / 2) - this.element.content.getHeight()/2 - 30;
			if(this.options.top < 10){
				this.options.top = 10;
			}
		}
		this.element.setStyle({left:this.options.left+"px",top:this.options.top+"px"});
	},
		
	validate : function(){
		if(this.options.onValidate){
			if(!this.options.onValidate(this)){
				return false;
			}
		}
		var form = this.element.content.down("form");
		if(form){
			if(!form.validation){
				form.validation  = new Validation(form);
			}
			form.validation.reset();
			return form.validation.validate();
		}
		return true;
	},
	
	apply : function(){
		if(this.validate()){
			if(this.options.onApply){
				this.options.onApply(this);
			}
			if(this.options.afterApply){
				this.options.afterApply(this);
			}
			this.close();
		}
	},
	closed : false,
	close : function(){
		if(this.options.droponclose){
			this.closed = true;
			if(this.shader){
				this.shader.remove();
			}
			this.element.remove();
			if(this.options.registerlevel)comdiv.modal.remove(this);
		}else{
			this.hide();
		}
		
	},
	
	

});


