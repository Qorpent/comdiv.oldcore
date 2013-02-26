((function(){
	qweb.defineGlobal('comdiv.roles');
	Object.extend(comdiv.roles,{
		getMatrix : function(){
			return qweb.call({name:'roles.matrix', type:'js'});
		},
		__getArgs : function(target, user, role){
			if(typeof(target)=="object")return target;
			return {target:target,user:user,role:role};
		},
		getRoleState : function(target, user, role, callback){
			try{
				result = qweb.call('roles.getstate',this.__getArgs(target,user,role));
				if(callback){
					callback(result);
				}
				return result;
			}catch(e){
				this.__catch(e);
			}
		},
		
		getMassRoleState : function(callback){
			try{
				result = qweb.call({name:'roles.getmassstate', type:'js'},{});
				if(callback){
					callback(result);
				}
				return result;
			}catch(e){
				this.__catch(e);
			}
		},
		addRole : function(target, user, role, callback){
			try{
				result =  qweb.call('roles.addrole',this.__getArgs(target,user,role));
				if(callback){
					callback(result);
				}
				return result;
			}catch(e){
				this.__catch(e);
			}
		},
		removeRole : function(target, user, role, callback){
			try {
				result = qweb.call('roles.removerole',this.__getArgs(target,user,role));
				if(callback){
					callback(result);
				}
				return result;
			}catch(e){
				this.__catch(e);
			}
		},
		__catch : function(e){
			if(comdiv.log){
					comdiv.log.error(e);
				}else{
					alert(e);
				}
				throw e;
		}
	});
})())