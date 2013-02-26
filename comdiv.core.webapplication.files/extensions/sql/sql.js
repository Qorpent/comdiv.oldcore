_result = (function () {
    var result = {

		execute_file : function(){
			this.execute(zeta.files.editor.getCurrentText(), true);
		},
	
		executeresource : function(assemblyname, resourcename, system,database, dialog, silent, callback){
			var parameters = {
				assemblyname : assemblyname,
				resourcename : resourcename,
                database: database || $(this.parameters.dbinput).value,
                system: system || $(this.parameters.sysinput).value,
            };
			if(!silent){
				if(dialog){
					new comdiv.modal.dialog({
						title : "Результаты запроса",
						modal : false,
						action : "sql/executeresource",
						parameters : parameters,
					});
				}else{
				Ajax
					.from("sql", "executeresource")
					.params(parameters)
					.update(this.parameters.result);
				}
			}else{
				Ajax
					.from("sql", "executeresource")
					.params(parameters)
					.after(function(r){
					if(callback){
							callback(r);
						}
					})
					.call();
			}
		},
		
		getresources : function(skipdialog){
			comdiv.modal.prompt("Имя сборки",function(r){
				new comdiv.modal.dialog({
					action : "sql/resources",
					parameters : {"assemblyname":r},
					modal :false,
					title : "SQL-ресурсы "+r
				});
			},"ALL",false,false,skipdialog);	
		},
	
        execute: function (query, dialog, silent, system, database, callback) {
            var parameters = {
                database: database || $(this.parameters.dbinput).value,
                system: system || $(this.parameters.sysinput).value,
                query: query || $(this.parameters.query).value
            };
			if(!silent){
				if(dialog){
					new comdiv.modal.dialog({
						title : "Результаты запроса",
						modal : false,
						action : "sql/execute",
						parameters : parameters,
					});
				}else{
				Ajax
					.from("sql", "execute")
					.params(parameters)
					.update(this.parameters.result);
				}
			}else{
				Ajax
					.from("sql", "execute")
					.params(parameters)
					.after(function(r){
					if(callback){
							callback(r);
						}
					})
					.call();
			}
			
        },
		
		getCurrentSystem : function(){
			return $(this.parameters.sysinput).value;
		},
		
		getCurrentDatabase : function(){
			return $(this.parameters.dbinput).value;
		},
		

        updateDatabases: function () {
            Ajax
				.from("sql", "databases")
				.param("system", $(this.parameters.sysinput).value)
				.after((function () {
				    $(this.parameters.dbinput).value = this.parameters.profile.data.database;
				}).bind(this))
				.update(this.parameters.dbinput);


        },

        updateConnections: function (callback) {
            Ajax
				        .from("sql", "connections")
				        .after((function () {
				            $(this.parameters.sysinput).value = this.parameters.profile.data.system;
				            this.updateDatabases();
							if(callback){
								callback(this);
							}
				        }).bind(this))
				        .update(this.parameters.sysinput);


        },

		addConnection: function () {
			this.doConnection("addconnection");
        },
		
		removeConnection: function () {
			this.doConnection("removeconnection");
        },

        doConnection: function (command) {
			$(this.parameters.connection).removeClassName("invalid");
            var c = $(this.parameters.connection).value;
            if (!c) {
                $(this.parameters.connection).addClassName("invalid");
				zeta.sys.shortResult.update(command, "Нужно указать строку подключения");
				return;
            }
            Ajax
			.from("sql", command)
            .param("connection", $(this.parameters.connection).value)
			.after((function (r) {
			    zeta.sys.shortResult.update(command, r.responseText);
			    this.updateConnections();
			}).bind(this))
			.call();
        },

        parameters: {
            execbutton: "main-toolbar-button-bd_execute",
            sysinput: "sql_system",
            dbinput: "sql_database",
            query: "sql_query",
            result: "sql_result",
            connection: "sql_connection",
            connectionadd: "sql_connection_add",
            connectionremove: "sql_connection_remove"

        },

        prepare: function (options) {
            Object.extend(this.parameters, options || {});
            if (!(this.parameters.profile)) {
                this.parameters.profile = zeta.profile.load("sql", {
                    system: "Default",
                    database: ""
                },"local");
            }

            $(this.parameters.sysinput).value = this.parameters.profile.data.system;
            Event.observe($(this.parameters.sysinput), "change", (function () {
                this.parameters.profile.data.system = $(this.parameters.sysinput).value;
                this.updateDatabases();
            }).bind(this));
            this.updateDatabases();

            Event.observe($(this.parameters.dbinput), "change", (function () {
                this.parameters.profile.data.database = $(this.parameters.dbinput).value;
            }).bind(this));

			Event.observe($(this.parameters.connectionadd), "click", (function () {
                this.addConnection();
            }).bind(this));

			Event.observe($(this.parameters.connectionremove), "click", (function () {
                this.removeConnection();
            }).bind(this));


            this.parameters.profile.autosave();
			if(zeta.application.controller=="sql"){
				//zeta.toolbar.show('bd');
				zeta.toolbar.updateTitle('Консоль SQL', 'Прямой вызов запросов SQL на сконфигурированных серверах');
				
				Event.observe(window,"keydown",function(e){
					if(e.shiftKey && e.keyCode==116){
						Event.stop(e);
						zeta.sql.execute();
						return false;
					}
				});
			}
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
zeta.sql = zeta.sql ? zeta.sql : {};
Object.extend(zeta.sql,_result());
zeta.sql.initialize();