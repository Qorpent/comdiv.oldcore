var Editor = {
	__myEditor : null,
	getEditor : function(){
		return Editor.__myEditor;
	},
	setEditor : function(e){
		
		Editor.__myEditor = e;
	},
	dropEditor : function(e){
		if(!Editor.__myEditor)return;
		try{
			Editor.__myEditor.destroy();
		}
		catch(e)
		{
		}
		Editor.__myEditor = null;
	},
	codeState : 'off',
	recreate : function(w,h){
		Editor.dropEditor();
		Editor.prepare(w,h);
	},
	inprepare :false,
	prepare : function(w,h){
		if(Editor.getEditor())return;
			if(Editor.inprepare)return;
			Editor.inprepare = true;
				//Setup some private variables
				var Dom = YAHOO.util.Dom,
					Event = YAHOO.util.Event;
					if (!h) h = '200px';
					if (!w) w = '450px';
					var h = '200px';
					var myConfig = {
						height: h,
						width: w,
						animate: true,
						dompath: true,
						focusAtStart: false
					};

				var myedit = new YAHOO.widget.Editor('editor', myConfig);
				
				  myedit.on('toolbarLoaded', function() {
						        var codeConfig = {
						            type: 'push', label: 'Правка HTML', value: 'editcode'
						        };
						       
						        this.toolbar.addSeparator();
						        this.toolbar.addButtonToGroup(codeConfig, 'insertitem');
						        
								this.lastEventKeys = {}
								
								var le = this.lastEventKeys;
								
								this.toolbar.comdiv_onCacheButtonClickEvent = function(e){
									le.shift = e.shiftKey;
								}
								
								var getSel = function(){
									var selection = myedit._getSelection();
									var result = selection.toString().stripTags();
									if (!myedit.lastEventKeys.shift) selection.deleteFromDocument();
									return result;
								}
								 
						        this.toolbar.on('editcodeClick', function() {
						            var ta = this.get('element'),
						                iframe = this.get('iframe').get('element');
						 
						            if (Editor.codeState == 'on') {
						                Editor.codeState = 'off';
						                this.toolbar.set('disabled', false);
						                this.setEditorHTML(ta.value);
						                if (!this.browser.ie) {
						                    this._setDesignMode('on');
						                }
						 
						                Dom.removeClass(iframe, 'editor-hidden');
						                Dom.addClass(ta, 'editor-hidden');
						                this.show();
						                this._focusWindow();
						            } else {
						                Editor.codeState = 'on';
						                this.cleanHTML();
						                Dom.addClass(iframe, 'editor-hidden');
						                Dom.removeClass(ta, 'editor-hidden');
						                this.toolbar.set('disabled', true);
						                this.toolbar.getButtonByValue('editcode').set('disabled', false);
						                this.toolbar.selectButton('editcode');
						                this.dompath.innerHTML = 'Editing HTML Code';
						                this.hide();
						            }
						            return false;
						        }, this, true);
					 
					        this.on('cleanHTML', function(ev) {
					            this.get('element').value = ev.html;
					        }, this, true);
					        
					        this.on('afterRender', function() {
					            var wrapper = this.get('editor_wrapper');
					            wrapper.appendChild(this.get('element'));
					            this.setStyle('width', '100%');
					            this.setStyle('height', '100%');
					            this.setStyle('visibility', '');
					            this.setStyle('top', '');
					            this.setStyle('left', '');
					            this.setStyle('position', '');
					 
					            this.addClass('editor-hidden');
					        }, this, true);
					    }, myedit, true);

				
				Editor.setEditor(myedit);
				
				
				Editor.getEditor().render();
				
				Editor.inprepare = false;
	}
}