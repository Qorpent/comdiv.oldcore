var zeta = (undefined != zeta) ? zeta : {};
zeta.toolbar = (undefined != zeta.toolbar) ? zeta.toolbar : {};
Object.extend(zeta.toolbar, {
	eventHandlersInstalled : false,
	overlays : [],
	init : function (options) {
		Object.extend (this, options || {});
		$$('.main-toolbar-expand').each(function(e){e.hide();});
		if (!this.eventHandlersInstalled){
			$$('.main-toolbar-tab').each( function(e) {
				Event.observe(e, "click", (function () {
					zeta.toolbar.show(e.readAttribute('code'),true);
				}).bind(this));	
			});
			this.eventHandlersInstalled  = true;
		}
		this.recalculateBodyMargin();
		this.element = $('main-toolbar');
		this.currentTab = "";
		for (code in this.collection) {
			e = $('main-toolbar-button-' + code);
			e.buttoncode = code;
			e.wikibutton = $('main-toolbar-button-' + e.buttoncode + '-wiki');
			e.wikibutton.main = e;
			e.hasdoc = e.getAttribute('hasdoc')=="True";
			e.doccode = e.getAttribute('doc');
			Event.observe (e, "click", (function (event) {zeta.toolbar.collection[this.buttoncode](this.buttoncode,event);}).bind(e));
			Event.observe ('main-toolbar-button-' + e.buttoncode + '-favorite', 'click', (function(event) {
				event.stop();
				zeta.toolbar.addtofavorite(this.buttoncode,event);
			}).bind(e));
			Event.observe (e.wikibutton, 'click', (function(event) {
				Event.stop(event);
				if(this.hasdoc) {
					zeta.wiki.open(this.doccode);
				} else {
					zeta.wiki.edit(this.doccode);
				}
				return false;
			}).bind(e));
			this.overlays.each(function(f){
				f(e);
			});
		}
		
		Object.extend(this.parameters, options || {});
		if (!(this.parameters.profile)) {
			this.parameters.profile = zeta.profile.load('toolbarfavorites', {});
			for(button in this.parameters.profile.data) {
				if(this.collection[button]) this.addtofavorite(button);
			}
		}
		
	},
	subtitle : null,
	title : null,
	titlecomment : null,
	updateTitle : function(text, comment, subtitle){
		this.title = text || this.title;
		this.titlecomment = comment || this.titlecomment;
		this.subtitle  = subtitle || this.subtitle;
		text = this.title +" "+this.subtitle;
		$('main-document-title').update(text);
		$('main-document-title').setAttribute('title',this.titlecomment);
		$('main-document-title').addClassName('new');
		window.setTimeout(function(){
			$('main-document-title').removeClassName('new');
		},2000);
		document.title = text;
	},
	autoRecalculateBodyMargin : true,
	recalculateBodyMargin : function(){
		if(this.autoRecalculateBodyMargin){
			document.body.style.marginTop = $('main-toolbar').getHeight() + 'px';
		}
	},
	
	show : function(tabname,setpref) {
		if(this.currentTab == tabname){
			this.currentTab = "";
			$('main-toolbar-expand-'+tabname).hide();
			$$('.main-toolbar-tab').each(function(e){e.removeClassName('active')});		
		}else{
			$$('.main-toolbar-expand').each(function(e){e.hide();});
			$$('.main-toolbar-tab').each(function(e){e.removeClassName('active')});
			deftab = $$('.main-toolbar-tab')[0];
			if($('main-toolbar-tab-'+tabname)){
				deftab = $('main-toolbar-tab-'+tabname);
			}
			deftab.addClassName('active');
			this.currentTab = deftab.getAttribute('code');
			$('main-toolbar-expand-'+this.currentTab).show();
					
			if(setpref){
				localStorage.setItem('-comdiv-toolbar-active-tab',this.currentTab);
			}
		}
		this.recalculateBodyMargin();
	},
	
	collection : {},
	
	parameters: {},
	
	register : function(code, action, quick) {
		this.collection[code] = action;
		if(quick){
			this.__quickfirst[quick[0]] = true;
			this.__quicks[quick] = code;
		}
	},
	
	__quickfirstpressed : false,
	__quickfirstkey : "",
	__quickfirst : {},
	__quicks : {},
	
	addtofavorite : function(code) {
		this.parameters.profile.data[code] = true;
		var div = new Element('div', {'class': 'main-toolbar-favorite', 'id': 'main-toolbar-favorite-' + code, 'title': $('main-toolbar-button-' + code).down().readAttribute('title')});
		var img = new Element('img', {'src': $('main-toolbar-button-' + code).down('img').readAttribute('src')});
		div.buttoncode = code;
		$('main-toolbar-button-' + code + '-favorite').addClassName('added');
		$('main-toolbar-button-' + code + '-favorite').stopObserving();
		Event.observe (div, "click", (function () {zeta.toolbar.collection[this.buttoncode](this.buttoncode);}).bind(div));
		Event.observe ('main-toolbar-button-' + div.buttoncode + '-favorite', 'click', (function(event) {
			event.stop();
			zeta.toolbar.removefromfavorite(this.buttoncode);
		}).bind(div));
		div.appendChild(img);
		$('main-toolbar-tabs').insert({
			top: div,
		});
		this.parameters.profile.save();
	},
	
	removefromfavorite : function(code) {
		delete this.parameters.profile.data[code];
		var e = $('main-toolbar-button-' + code + '-favorite');
		e.buttoncode = code;
		$(e).removeClassName('added');
		$(e).stopObserving();
		$('main-toolbar-favorite-' + code).remove();
		Event.observe ('main-toolbar-button-' + e.buttoncode + '-favorite', 'click', (function(event) {
			event.stop();
			zeta.toolbar.addtofavorite(e.buttoncode);
		}).bind(e));
		this.parameters.profile.save();
	},
});
var __CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
var __CHARDELTA = 65
Event.observe(window, 'load', function(){
	zeta.toolbar.init();
	$$(["textarea","input"])[0].focus();
	window.setTimeout(function(){
		t = localStorage.getItem('-comdiv-toolbar-active-tab'); 
		if(t){
			if(zeta.toolbar.currentTab!=t){
				zeta.toolbar.show(t);
			}
		}
	},200);
	Event.observe(window,"keydown",function(e){
		
		if(e.shiftKey && e.altKey && e.keyCode!=16 && e.keyCode!=17 && e.keyCode!=18 && e.keyCode!=27 && e.keyCode!=91){
				
			c = __CHARS[e.keyCode - __CHARDELTA];
			//if(zeta.sys.debug){
				zeta.sys.log.append("sys keydown entered with char '"+c+"'");
			//}
			if(this.__quickfirstpressed){
				quickcode = this.__quickfirstkey+ c;
				this.__quickfirstpressed = false;
				this.__quickfirstkey = "";
				if(this.__quicks[quickcode]){
					zeta.sys.log.append("quick command found with '"+this.__quicks[quickcode]+"' code found");
					Event.stop(e);
					this.collection[this.__quicks[quickcode]]();
					return false;
				}
			}else{
				if(this.__quickfirst[c]){
					
					Event.stop(e);
					this.__quickfirstpressed = true;
					this.__quickfirstkey = c;
					window.setTimeout(function(){
						zeta.sys.log.append("timeout ended");
						this.__quickfirstpressed = false;
						this.__quickfirstkey = "";
					}.bind(this),1000);
					//if(zeta.sys.debug){
						zeta.sys.log.append("quick command start found , wait started ");
					//}
					return false;
				}
			}
		}
	}.bind(zeta.toolbar))
})