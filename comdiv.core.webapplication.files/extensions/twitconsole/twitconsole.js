zeta = zeta ? zeta : {};
zeta.twit = zeta.twit ? zeta.twit : {};
Object.extend(zeta.twit, {
    send: function (text) {
        if(!text){
            text = this.sendinput.value;
        }
        if(!text){
            zeta.sys.shortResult.update('Твиттер, отправка', 'Укажите текст');
            this.sendinput.focus();
            return;
        }
        Ajax.from('twitconsole','send').param('text',text).after(
            function(r){
                zeta.sys.shortResult.update('Твиттер, отправка', r.responseText);
                this.sendinput.value = '';
            }.bind(this)
        ).call();
    },

    init : function(opts){
        Object.extend(this.options,opts || {});
        this.sendinput = $(this.options.sendinput);
        Event.observe(this.sendinput, 'keydown', function(event){
            if(event.keyCode == 13){
                this.send();
                Event.stop(event);
                return false;
            }
        }.bind(this));
        this.popup = $(this.options.popup);
		this.history = $(this.options.history);
		this.twitterbutton = $(this.options.twitterbutton);
		this.historybutton = $(this.options.historybutton);
        this.popup.inner = Selector.findChildElements(this.popup, ['.twit-popup-inner',])[0];
		this.history.inner = $('twitter_console_inner');
        Event.observe($(this.twitterbutton), 'click', (function(e){
            this.send();
        }).bind(this));
		Event.observe($(this.historybutton), 'click', (function(e){
            this.gethist();
        }).bind(this));
    },
    sendinput : null,
    popup : null,
    popup : null,
    options : {
        sendinput : 'twitter_console_send_text',
        popup : 'twitter_console_popup',
        twitterbutton : 'twitter-send-button',
		historybutton : 'twitter-history-button',
		history : 'twitter_console_history',
    },

	gethist : function() {
		Ajax.from('twitconsole','history').after(
			function(r){
				text = r.responseText;
                this.history.inner.insert({top : text});
				this.history.show();
            }.bind(this)
		).call();
	},

    check : function(){
        Ajax.from('twitconsole','check').after(
            function(r){
                text = r.responseText;
                if(text){
                    this.popup.inner.insert({top : text});
                    this.popup.show();
                }
            }.bind(this)
        ).call();
    },

    autocheck : function(seconds){
        window.setInterval(function(){this.check();}.bind(this), (seconds || 60) * 1000);
    },
});
Event.observe(window,'load', function(){
    zeta.twit.init();
    zeta.twit.popup.hide();
	zeta.twit.history.hide();
    zeta.twit.check();
    zeta.twit.autocheck();
});