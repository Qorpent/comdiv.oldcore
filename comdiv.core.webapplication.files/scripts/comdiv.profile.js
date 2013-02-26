///// THIS MODULE IS JS CLIENT FOR STANDARD COMDIV PROFILE CONTROLLER   ////
///// CAN BE LOADED BOTH IN NATIVE SCRIPT MODE OR IN EVAL(...) BY AJAX  ////
//@requires prototype
//@requires comdiv.fluentajax



comdiv_profile_installer = function(target){
	Object.extend(target,{
	
		//здесь хранятся кэшированные профили
		cache : {
		
		},
		
		//при загрузке берет профиль по умолчанию, сохраняет его
		//потом берет из сети, накатывает его на текущий и сохраняет
		//итог  - save уже сам разберется, требуется ли сохранение или
		//нет
		load : function(code, defaultprofile){
			var self = this;
			if(this.cache[code])return this.cache[code];
			var profile = {
				code : code,
				data : defaultprofile || {},
				save : function(){
					self.save(this);
				},
				
				lastJSON : "",
				evalJSON : function(){
					return $H(this.data).toJSON();
				},
				
				autosave : function(seconds){
					time = (seconds || 10) * 1000;
					var self = this;
					window.setTimeout(function(){
						self.save();
						self.autosave();
					},time);
				}
			}
			var data = this._load(code);
			profile.lastJSON = $H(data).toJSON();
			Object.extend(profile.data,data);
			profile.save();
			return profile;
		},
		
		//сверяет объект в кэше с текущим, если есть разница (сверяет JSON), то записывает 
		//новое значение в кэш и вызывает реальную операцию сохранения на сервере
		save : function(profile){
			if(profile.lastJSON!=profile.evalJSON() ){
				this._save(profile);
			}
		},
		
		_load : function(code){
			var result = Ajax.from("profile","load").param("code",code).eval();
			return result;
		},
		
		
		_save : function(profile){
			if(!profile.savemode){
				profile.savemode = true; 
				Ajax.from("profile","save")
					.post()
					.param("code",profile.code)
					.param("content",profile.evalJSON())
					.after(function(){
						profile.lastJSON = profile.evalJSON();
						profile.savemode = false;
					})
					.error(function(){
						profile.savemode = false;
					})
					.call();
			}
		}
	
	});
	return target;
};
var zeta = (undefined != zeta ) ? zeta : {}
zeta.profile = (undefined != zeta.profile) ? zeta.profile : {}
comdiv_profile_installer(zeta.profile); //execute on real target in native mode
comdiv_profile_installer; //force to return as function in JSON mode