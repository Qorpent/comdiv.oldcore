var zeta = zeta ? zeta : {};
zeta.mas = zeta.mas ? zeta.mas : {};
Object.extend(zeta.mas,{
	run: function (options){
		options = Object.extend({
			name:null,
			args : "",
			targetapp : "",
			after:  Prototype.emptyFunction,
			onerror: Prototype.emptyFunction,
			
		},options||{});
		if(!options.name){
			comdiv.modal.alert("Не указано имя запускаемого поцесса");
			return;
		}
		zeta.sys.log.append("Стартую вызов процесса "+Object.toJSON(options));
		Ajax
			.from("mas/run")
			.params(options)
			.after(	function(r){
					zeta.sys.log.append("Вызов успешен code="+r.responseText);
					zeta.mas.uiproc(r.responseText);
					options.after(r);})
			.error(	function(r){
					zeta.sys.log.append("Вызов завершен с ошибкой error="+r.responseText);
					options.onerror(r);})
			.call();
	},
	execute : function(options){
		options = Object.extend({
			app : "",
			command : "",
			after:  Prototype.emptyFunction,
			onerror: Prototype.emptyFunction,
			
		},options||{});
		if(!options.app){
			comdiv.modal.alert("Не указан код приложения");
			return;
		}
		if(!options.command){
			comdiv.modal.alert("Не указана комманда");
			return;
		}
		zeta.sys.log.append("Стартую вызов комманды "+Object.toJSON(options));
		Ajax
			.from("mas/executecommand")
			.params(options)
			.after(	function(r){
					zeta.sys.log.append("Вызов успешен");
					options.after(r);})
			.error(	function(r){
					zeta.sys.log.append("Вызов завершен с ошибкой error="+r.responseText);
					options.onerror(r);})
			.call();
	},
	getstate : function(code){
		return Ajax.from("mas/getstate").param("processcode",code).eval();
	},
	uiproc : function(code){
		if($('processstatus')){
			e = $('processstatus');
			s = new Element("div",{"class":"processstate",state:"-100",title : code});
			s.code = code;
			s.state = -100;
			s.finished = false;
			s.updatestate = function(){
				res = zeta.mas.getstate(this.code);
				r = isNaN(res.Result) ? -100 : res.Result;
				this.setAttribute("state",r);
				this.state = r;
				sname = "Ошибка";
				if(r==-100){
					sname = "Запуск";
				}
				if(r==-10){
					sname = "Выполненяется";
				}
				if(r==0){
					sname = "Выполнен";
				}
				if(r==2){
					sname = "Дублирующий процесс";
				}
				if(res.name){
					this.setAttribute("title",res.Name+" "+res.Args+" "+sname);
				}
				if(typeof(res.Id)=="number"){
					this.id = res.Id;
				}
				if(typeof(res.IsActive)=="boolean" && !res.IsActive){
					this.finished = true;
				}
				if(!this.finished){
					window.setTimeout(function(){
						this.updatestate();
					}.bind(this),2000);
				}
			}.bind(s);
			
			Event.observe(s,"dblclick",function(){
				this.inclose = true;
				this.finished = true;
				this.remove();
				delete(this);
			}.bind(s));
			
			Event.observe(s,"click",function(){
				
				if(this.id){
					window.setTimeout(function(){
						if(this.inclose)return;
					id = this.id;
					 new comdiv.modal.dialog({
								 title : "Просмотр журнала процесса "+id,
								 action : "hql/execute",
								 parameters : {query : "from ProcessLog where Process.Id = "+id+" order by Id", system : 'mas', nosetquery : true},
								 modal : false,
								 width: 800,
								 height:400,
							 });
					}.bind(this),500);
				}
			}.bind(s));
			
			e.appendChild(s);
			window.setTimeout(function(){
				s.updatestate();
			}.bind(this),1000);
		}
	},
	send:function(options){
		options = Object.extend({
			type : "",
			message : "",
			id : 0,
			sender : "",
			after : Prototype.emptyFunction,
			onerror : Prototype.emptyFunction,
		},options||{});
		if(!options.type){
			comdiv.modal.alert("Не указан тип сообщения");
			return;
		}
		zeta.sys.log.append("Запуск сообщения "+Object.toJSON(options));
		Ajax
			.from("mas/run")
			.params(options)
			.after(	function(r){
					zeta.sys.log.append("Сообщение зарегистрировано Id="+r.responseText);
					options.after(r);})
			.error(	function(r){
					zeta.sys.log.append("Вызов сообщения завершен с ошибкой error="+r.responseText);
					options.onerror(r);})
			.call();
	},
	clean : function(options){
		options = Object.extend({
			resultcondition : "Eq",
			result : 0,
			active : false,
			name : "",
			host : "",
			args : "",
			timecondition : "None",
			time  : "",
			after : Prototype.emptyFunction,
			onerror : Prototype.emptyFunction,
			__fields : {
				result : {name: "Результат процесса (код выхода), 0 - нормальный останов",},
				active : {name: "Включать в очистку активные процессы",},
				resultcondition : {name:"Операция поиска результата" ,
					list : {
						None : "Нет",
						Eq : "Равен",
						Neq : "Не равен",
						Lt : "Меньше",
						Gе : "Больше или равен",
					},
				},
				name : {name:"Маска имени, может включать %",},
				host : {name: "Хост (целевой компьютер)",},
				args : {name:"Маска параметров, может включать %",},
				timecondition : {name:"Операция поиска по времени начала",
					list : {
						None : "Нет",
						Le : "Меньше или равно",
						Gе : "Больше или равно",

					},
				},
				time : {name:"Время начала",}
				
			}
		},options||{});
		comdiv.modal.editobject("Выполнить очистку",options,function(opts){
			zeta.sys.log.append("Запуск очистки "+Object.toJSON(opts));
			Ajax
				.from("mas/clean")
				.params(opts)
				.after(	function(r){
						zeta.sys.log.append("Очистка проведена успешно");
						opts.after(r);})
				.error(	function(r){
						zeta.sys.log.append("Вызов очистки завершен с ошибкой error="+r.responseText);
						opts.onerror(r);})
				.call();
		});
	},
	
	getCurrentApplication : function(){
		if($('currentapplication')){
			return $('currentapplication').value;
		}
		return localStorage.getItem("_mas_current_app");
	},
	
	setCurrentApplication : function(value){
		if(this.__currentApplication){
			this.__currentApplication.value = value;
		}
		localStorage.setItem("_mas_current_app", value);
	},
	
	onwindowload : function(){
		if($('currentapplication')){
			this.__currentApplication = $('currentapplication');
			Event.observe(this.__currentApplication,"change",function(){
				this.setCurrentApplication(this.__currentApplication.value);
			}.bind(this));
			if(localStorage.getItem("_mas_current_app")){
				this.setCurrentApplication(localStorage.getItem("_mas_current_app"));
			}
		}
	},
});
Event.observe(window,"load",function(){
	zeta.mas.onwindowload();
});