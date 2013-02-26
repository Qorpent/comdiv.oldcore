((function(){
	qweb.defineGlobal('comdiv.roles.ui');
	Object.extend(comdiv.roles.ui,{
		massSetCells : function(cells){
			alldata = comdiv.roles.getMassRoleState();
			for(var i=0;i<alldata.length;i++){
				val = alldata[i];
				e = $(val.id);
				if(e){
					e.setState(val.value);
				}
			}
		},
		extendCell : function(cell){
			Object.extend(cell, {
				setState : function(state) {
					
					if(typeof(state)=="undefined"){
						comdiv.roles.getRoleState(this.roleObject,null,null,function(state){
							this.state = state;
							this.__setUIstate();
						}.bind(this));
						return;
					}
					this.state = state;
					this.__setUIstate();
					
				},
				
				__setUIstate : function(){
					this.removeClassName("state_true");
					this.removeClassName("state_false");
					this.addClassName("state_"+this.state);				
				},
				
				changeState : function(){
					if(this.state){
						comdiv.roles.removeRole(this.roleObject);
					}else{
						comdiv.roles.addRole(this.roleObject);
					}
					this.state = !this.state;
					this.__setUIstate();
				},
			});
		},
		
		
		
		extendTable : function(table){
			Event.observe(table,'mousedown',function(e){
				td = Event.findElement(e,'td');
				if(td){
					if(confirm('Вы уверены?')){
						td.changeState();
					}
				}
			});
		},
	});
	
	
})())