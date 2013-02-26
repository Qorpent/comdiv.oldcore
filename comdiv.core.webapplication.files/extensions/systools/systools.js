var zeta = zeta ? zeta : {};

zeta.sys = zeta.sys ? zeta.sys : {};
Object.extend(zeta.sys, {
	init : function(){
		this.ajaxStatus.init();
		this.shortResult.init();
		this.ping.init();
		this.security.init();
    },	
});

zeta.sys.ajaxStatus = zeta.sys.ajaxStatus ? zeta.sys.ajaxStatus   : {};
Object.extend(zeta.sys.ajaxStatus , {
	counter : 0,
	requests  : {} ,
	element : null,
	init : function(){
		this.element =  $('zeta_ajax_status');
		Ajax.Responders.register({
			onCreate : (function(req){
					this.enter(req);
				}).bind(this),
			onComplete : (function(req){
					this.leave(req);
				}).bind(this)
		});
	},
	enter : function(req){
		this.requests[req.url] = req;
		this.element.removeClassName('inactive');
		this.element.addClassName('active');
		this.updateTitle();
	},
	leave : function(req){
		delete(this.requests[req.url]);
		if(Ajax.activeRequestCount == 0){
			this.element.removeClassName('active');
			this.element.addClassName('inactive');
		}		
		this.updateTitle();
	},
	updateTitle : function(){
		var title = "";
		if (Ajax.activeRequestCount == 0){
			title = "Вращается при выполнении Ajax запросов";
		}else{
			for (var i in this.requests){
				title += i + ";   ";
			}
		}
		this.element.setAttribute('title', title);
	},
});

zeta.sys.shortResult = zeta.sys.shortResult ? zeta.sys.shortResult : {};
Object.extend(zeta.sys.shortResult,{
	init : function(){
		this.element = $('status-short-result');
	},
	clear : function(){
		this.element.update("");
	},
	update : function(title,text){
		this.element.update(text);
		this.element.addClassName('new');
		this.element.setAttribute('title',title);
		window.setTimeout((function(){this.element.removeClassName('new');}).bind(this),2000);
	},
	request : function(controller,action,params){
		params = params || {};
		this.clear();
		Ajax
			.from(controller,action)
			.sync()
			.params(params)
			.after((function(req){this.update(req.request.url,req.responseText);}).bind(this))
		.call();
	},
});

zeta.sys.log = zeta.sys.log ? zeta.sys.log : {};
Object.extend(zeta.sys.log,{
	append : function(text){
		var e = new Element("div", {"class" : "logmessage"});
		e.update(text);
		$('zeta_sys_log_div').appendChild(e);
	},
	show : function(){
		new comdiv.modal.dialog({
			title : "Журнал событий JS",
			targetelement : $('zeta_sys_log_div'),
		});		
	},
	clear : function(){
		$('zeta_sys_log_div').update("");
	},
});


zeta.sys.ping = zeta.sys.ping ? zeta.sys.ping : {};
Object.extend(zeta.sys.ping,{
	status : 0,
	init : function(){
		this.element = $('zeta_ping_status');
		Event.observe(this.element,"click", (function(){
			this.test();
		}).bind(this));
		this.autotest();
		this.test();
		this.waserror = false;
	},
	
	autotest : function(){
		window.setTimeout( ( function(){
			this.test();
			this.autotest();
		}).bind(this), 1000*60*10);
	},
	
	test : function(){
		this.element.removeClassName("noping");
		this.element.removeClassName("ping");
		this.element.addClassName("unk");
		this.waserror = false;
		this.status = 0;
		Ajax.from("echo","get")
			.sync()
			.after((function(r){
				if(r.status == 0){
					this.onerror();
				}else{
					if(!this.waserror){
						this.onok();
					}
				}
			}).bind(this))
			.error((function(){
				this.onerror();
			}).bind(this))
			.call();
	},
	
	onerror : function(){
		this.element.removeClassName("ping");
		this.element.addClassName("noping");
		zeta.sys.shortResult.update("PING","ОШИБКА СВЯЗИ");
		this.waserror = true;
		this.status = -1;
	},
	onok : function(){
		this.element.removeClassName("noping");
		this.element.addClassName("ping");
		zeta.sys.shortResult.update("PING","OK");
		this.status = 1;
	},
	
});


zeta.sys.security = zeta.sys.security ? zeta.sys.security : {};
Object.extend(zeta.sys.security ,{
	impersonate : function(){
		this.username.removeClassName('invalid');
		if(!this.username.value){
			this.username.addClassName('invalid');
			zeta.sys.shortResult.update("imersonate","Укажите пользователя!");
		}else{
		Ajax.from('impersonate','enter')
			.param("username",this.username.value)
			.after(function(){document.location=document.location})
			.call();
		}
	},
	
	currentuserClick : function(){
		if($('status-current-user').getAttribute("isadmin")=="True"){
			zeta.toolbar.show('sys');
			this.username.focus();
		}
	},	
	
	assign : function(){
		this.execute('assign',true);
	},
	test : function(){
		this.execute('test',true);
	},
	revoke : function(){
		this.execute('revoke',true);
	},
	execute : function(command, needrole){
		this.username.removeClassName('invalid');
		this.role.removeClassName('invalid');
		if(!this.username.value){
			this.username.addClassName('invalid');
			zeta.sys.shortResult.update(command,"Укажите пользователя!");
		}else if(!this.role.value && needrole){
			this.role.addClassName('invalid');
			zeta.sys.shortResult.update(command,"Укажите роль!");
		}
		else {
			zeta.sys.shortResult.request("role",command,{username : this.username.value, role:this.role.value});
		}
	},
	
	init : function(){
		this.table = $('core-security-table');
		if (this.table){
			this.username = $('core-security-username');
			this.role = $('core-security-role');
			Event.observe($('core-security-impersonate'),"click",(function(){
				this.impersonate();
			}).bind(this));
			Event.observe($('core-security-test'),"click",(function(){
				this.test();
			}).bind(this));
			Event.observe($('core-security-assign'),"click",(function(){
				this.assign();
			}).bind(this));
			Event.observe($('core-security-revoke'),"click",(function(){
				this.revoke();
			}).bind(this));
			
			Event.observe($('status-current-user'),"click",(function(){
				this.currentuserClick();
			}).bind(this));

		}
	},
});

Event.observe(window,"load",function(){ zeta.sys.init(); });