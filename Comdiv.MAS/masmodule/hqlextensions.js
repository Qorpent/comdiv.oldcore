
Event.observe(window, 'load', function () {
	if(zeta.hql){
		zeta.hql.registerOnMenuHandler(function(e,menu,id,entity){
			if(entity=="Process"){
				menu.list.addItem({
					name : "Просмотр журнала",
					callback : function(){
						 new comdiv.modal.dialog({
							 title : "Просмотр журнала процесса "+id,
							 action : "hql/execute",
							 parameters : {query : "from ProcessLog where Process.Id = "+id+" order by Id desc", system : 'mas', nosetquery : true},
							 modal : false,
						 });
						
					},
				});
				menu.list.addItem({
					name : "Просмотр событий",
					callback : function(){
						 new comdiv.modal.dialog({
							 title : "Просмотр журнала событий процесса "+id,
							 action : "hql/execute",
							 parameters : {query : "from ProcessMessage where Process.Id = "+id+" order by Id desc", system : 'mas'},
							 modal : false,
						 });
						
					},
				});

				menu.list.addItem({
					name : "Остановить",
					callback : function(){
						Ajax.from("mas/send").params({
							id:id, type : "quit", message : "manual call" ,
						}).after(function(){comdiv.modal.message("Останов послен процессу")})
						.call();
					},
				});
				
				menu.list.addItem({
					name : "Принудительный останов",
					callback : function(){
						Ajax.from("mas/send").params({
							id:id, type : "kill", message : "manual call" ,
						}).after(function(){comdiv.modal.message("Принудительный останов послен процессу")})
						.call();
					},
				});

			}		
			
		});
	}
});        