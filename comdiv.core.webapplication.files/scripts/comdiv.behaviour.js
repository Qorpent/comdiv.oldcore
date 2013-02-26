Behaviour.register({
	'[targetElement]' : function(e){
		var from = $(e);
		var to = $(from.getAttribute('targetElement'))
		$bind(from, to);
		to.value = from.value;
		
	}
});

Behaviour.register({
	'[onEnter]' : function(e){
		e = $(e);
		var enter = e.getAttribute('onEnter');
		var func = eval(enter);
		e.onEnter = function(){
			func(e);
		}
		Event.observe(e,'keydown',function(event){
			var e = Event.element(event);
			if (event.keyCode==13) {
				if(typeof(e.targetElement)!='undefined'){
					e.targetElement.value = e.value;
				}
				e.onEnter();
				event.stop();
				return false;
			}
			return true;
		});
		
	},
	
	'[onEsc]' : function(e){
		e = $(e);
		var enter = e.getAttribute('onEsc');
		var func = eval(enter);
		e.onEsc = function(){
			func(e);
		}
		Event.observe(e,'keydown',function(event){
			var e = Event.element(event);
			if (event.keyCode==27) {			
				e.onEsc();
				if(e.tagName == "INPUT"){
					setTimeout(function(){
						e.value = "";
					},100);
				}
				event.stop();
				return false;
			}
			return true;
		});
		
	}
		
});

var comdiv = comdiv || {};
comdiv.dynhtml = comdiv.dynhtml || {};

comdiv.dynhtml.__setesc = function(ed){
	comdiv.dynhtml.onesc(ed,function(){
		ed.target.removeClassName('changed');
		ed.target.removeClassName('not-changed');
		ed.target.update(ed.target.initialText);
		ed.target.editMode = "static";
	});

}

comdiv.dynhtml.__setchange = function(ed){
	var ev = "keyup";
	if(ed.tagName == "SELECT"){
		ev = "change";
	}
	Event.observe(ed,ev,function(){
		if(ed.value == ed.target.initialValue) {
			ed.target.removeClassName('changed');
			ed.target.addClassName('not-changed');
		}else{
			ed.target.addClassName('changed');
			ed.target.removeClassName('not-changed');
		}
	});
	Event.observe(ed,"custom:keyup",function(){
		if(ed.value == ed.target.initialValue) {
			ed.target.removeClassName('changed');
			ed.target.addClassName('not-changed');
		}else{
			ed.target.addClassName('changed');
			ed.target.removeClassName('not-changed');
		}
	});
}


comdiv.dynhtml.onenter = function(e,func){
	e = $(e);
	Event.observe(e,'keydown',function(event){
					if (event.keyCode==13) {
						func();
						event.stop();
						return false;
					}
					return true;
				});
}
comdiv.dynhtml.onesc = function(e,func){
	e = $(e);
	Event.observe(e,'keydown',function(event){
					if (event.keyCode==27) {
						func();
						event.stop();
						return false;
					}
					return true;
				});
}

comdiv.dynhtml.__commonprepare = function(e,ed,onenter){
	e = $(e);
	var t = e.getAttribute("value");
	e.initialValue = t;
	e.initialText = e.innerHTML;
	e.update("");
	e.editor = ed;
	ed.target = e;
	ed.value = t;
	e.appendChild(ed);
	e.editMode = "edit";
	ed.target.addClassName('not-changed');	
	comdiv.dynhtml.__setchange(ed);
	comdiv.dynhtml.__setesc(ed);
	if (onenter) {
		comdiv.dynhtml.onenter(ed,onenter);
	}
}

comdiv.dynhtml.convertToEdit = function(e,onenter){
	var ed = new Element("input");
	comdiv.dynhtml.__commonprepare(e,ed,onenter);
	return ed;
}

comdiv.dynhtml.convertToSelect = function(e,list,onenter){
	var ed = new Element("select");
	ed.update($(list).innerHTML);
	comdiv.dynhtml.__commonprepare(e,ed,onenter);
	return ed;
}