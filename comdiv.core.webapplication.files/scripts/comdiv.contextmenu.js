var ContextMenu = {
	utils : {
		listIds : function(){
			var contextIds = Selector.findChildElements(null,['[context_menu_id]',]);
			var results = [];
			contextIds.each(function(e){
				var contextId = e.getAttribute('context_menu_id');
				if(!results.include(contextId))results.push(contextId);
			});
			var win = window.open('about:blank','_clist');
			win.onload=function(){
				results.each(function(e){
					win.document.body.innerHTML += '<p>'+e+'</p>';
				});
			}
		}
	}
}
ContextMenu.onCreated = new YAHOO.util.CustomEvent('main_context_menu_created');
ContextMenu.onCreated.signature = YAHOO.util.CustomEvent.FLAT;
ContextMenu.onPopulate = new YAHOO.util.CustomEvent('main_context_populate');
ContextMenu.onPopulate.signature = YAHOO.util.CustomEvent.FLAT;
ContextMenu.onCreated.subscribe(function(menu){
	menu.onPopulate.subscribe(function(params){
	
		ContextMenu.onPopulate.fire(params);
	});
});

var mainContextMenu = null;
document.observe('dom:loaded', function(){
	YAEvent = YAHOO.util.Event;
	function resolveMenuHandler(e){
		var current = e;
		while(current  && !current.getAttribute('context_menu_id')) {
			current = current.parentNode;
			if(current == document.body) return null;
		}
		return current;
	}
		
	YAEvent.onDOMReady(function () {
		function position(p_sType, p_aArgs, p_aPos) {
			this.cfg.setProperty("xy", p_aPos);
			this.beforeShowEvent.unsubscribe(position, p_aPos);
		}
		var mainContextMenu = new YAHOO.widget.ContextMenu(
			"mainContextMenu",
			{
				trigger :document.body.id,
				itemdata : ["stub",],
				lazyload: true
			}
		);
		mainContextMenu.onPopulate =  mainContextMenu.createEvent('populate');
		mainContextMenu.onPopulate.signature = YAHOO.util.CustomEvent.FLAT;
		
		//hack
		mainContextMenu._removeEventHandlers();
		//новый обработчик
		mainContextMenu._onTriggerContextMenu =  function(p_oEvent, p_oMenu) {
			this.contextEventTarget = YAEvent.getTarget(p_oEvent);
			var contextMenuHandlerElement = resolveMenuHandler(this.contextEventTarget);
			if (!contextMenuHandlerElement) return;
			var aXY;
			YAEvent.stopEvent(p_oEvent);
			this.contextEventTarget = YAEvent.getTarget(p_oEvent);
			this.triggerContextMenuEvent.fire(p_oEvent);
			YAHOO.widget.MenuManager.hideVisible();
			if(!this._bCancelled) {
				aXY = YAEvent.getXY(p_oEvent);
				if (!YAHOO.util.Dom.inDocument(this.element)) {
					this.beforeShowEvent.subscribe(position, aXY);
				}
				else {
					this.cfg.setProperty("xy", aXY);
				}
				this.show();
				this.clearContent();
				this.onPopulate.fire({menu:this,info:contextMenuHandlerElement,target:this.contextEventTarget});
				this.render();
			}
			this._bCancelled = false;
		}
		mainContextMenu.configTrigger(null,[document.body.id,],null);
		
		ContextMenu.onCreated.fire(mainContextMenu);
                
    });
});

