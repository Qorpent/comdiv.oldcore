var ws = {
	toggle : function(id,effect){
		Effect.toggle($('wsob_'+id),effect);
		Effect.toggle($('wscb_'+id),effect);
		Effect.toggle($('wsi_'+id),effect);
	}
}
var Data = {}

var utils = {
	nocall_error : false,
	define : function(obj){
		if(typeof(obj)=="undefined"){
			return {};
		}
		return obj;
	},
	refresh : function(){
		document.location = document.location;
	},
	executecollapser : function(collapser){
		collapser = $(collapser);
		targetcss = collapser.getAttribute('targetcss');
		targetid = collapser.getAttribute('targetid');
		if(targetid){
			targetcss = "#"+targetid;
		}
		$$(targetcss).each(Element.toggle);
		collapser.toggleClassName('expanded');
		collapser.toggleClassName('collapsed');
		
		collapser.expanded = collapser.hasClassName('expanded');
		
		collapser.afterexecute(collapser);
	},
	
	onEnter : function(event,func){
		if(event.keyCode==13){
			event.stop();
			func();
			return false;
		}
		return true;
	},
	action : function(path){
		return _siteroot + "/" + path + ".rails";
	},
	call : function(action,parameters,callback,sync){
		var call = {}
		if(sync){
			var i = 1; //stop here
		}
		if(parameters){
			call.parameters = parameters;
		}
		if(callback){
			call.onSuccess = callback;
		}
		if(sync){
			call.asynchronous = false;
		}
		new Ajax.Request($$A(action),call);
	},
	post : function(action,parameters,callback){
		var call = {}
		if(parameters){
			call.parameters = parameters;
		}
		if(callback){
			call.onSuccess = callback;
		}
		call.method = "POST";
		new Ajax.Request($$A(action),call);
	},
	update : function(element,action,parameters,callback,sync){
		if($(element)){
			if(sync){
			var i = 1; //stop here
			}
			var call = {}
			call.parameters = parameters ? parameters : {};
			if(call.parameters.methodPost){
				call.method="POST";
			}
			if(callback){
				call.onUpdate = callback;
			}
			if(sync){
				call.asynchronous = false;
			}
			call.evalScripts = true;
			call.parameters.ajax = 1;
			new Ajax.Updater(element,$$A(action),call);
		}
	},
	bindelements : function(from,to){
		from = $(from);
		to = $(to);
		from.targetElement = to;
		Event.observe(from, 'change', function(event){
			var e = Event.element(event);
			e.targetElement.value = e.value;
		});
	},

__commaholder:0}

var $$A = utils.action;
var $$T = function(element){
	return undefined == element.textContent ? element.innerText : element.textContent;
}
var $$R = utils.call;
var $$POST = utils.post;
var $$U = utils.update;
var $url = function(action,object){
	return $$A(action)+"?"+$H(object).toQueryString();
}
var $bind = utils.bindelements;
var $newtab = function(url){
	window.open(url,'_blank');
}
var $relocate = function(url){
	if(!url)url = document.location;
	document.location = url;
}
var $refresh = function(){
	document.location = document.location;
}
var restoration = null;
var Globals = {
    SiteRoot : "",
    EnterPressed : function(event){
        return event.keyCode==13;
    },
    rollercounter : 0,
    inc_roller : function ()
        {
             $('roller').style.display = 'block';
             Globals.rollercounter+=1;
        },
    dec_roller : function () 
        {
            Globals.rollercounter-=1;
                if(Globals.rollercounter<0)Globals.rollercounter = 0;
                if(Globals.rollercounter==0)$('roller').style.display='none';
        }
}


Ajax.Responders.register({
    
    onCreate : Globals.inc_roller,
	
    onComplete : function(req){
        Globals.dec_roller();
        if(!req.success())
        {
			if (utils.nocall_error == false ){ 
			if(comdiv && comdiv.modal){
				new comdiv.modal.dialog({
					content : "Ошибка AJAX при выполнении запроса к "+req.url+", обратитесь к администратору<br><div>"+req.transport.responseText+"</div>",
					title : "Сообщение об ошибке",
					modal : true,
					dialogclass : "error",
				});
			}
			}
			if(restoration){
				restoration();
				restoration = null;
			}
        }
		if(req.transport.status == "0"){
			if(comdiv && comdiv.modal){
				new comdiv.modal.dialog({
					content : "Сервер недоступен",
					title : "Сообщение об ошибке",
					modal : true,
					width : 300,
					height: 150,
					dialogclass : "error",
				});
			}
			if(restoration){
				restoration();
				restoration = null;
			}
		}
		if(!/showerrors/i.match(req.url)){
		//Zeta.checkErrors();
		}
    },
	
	
	
    
});

function $et(e)
{
    var res = e.srcElement;
    if(!res) res = e.target;
    return res;
}
function $v(e)
{
    var el = $(e);
    if (el && el.value)
        return el.value;
}
function togCol(col,close)
	{
		var c1 = close ? $('col.'+col) : $('col.'+col+'2');
		var c2 = close ? $('col.'+col+'2') : $('col.'+col);
		
		c1.style.visibility = 'collapse';
		c1.style.border = 'none';
		c2.style.visibility = 'visible';
		c2.style.border = 'solid 1px black';
		return false;
	}

Globals.Behaviour = {

	UpdateAjaxForm : function(form,element,advancedParameters,skipAnim)
		{
			var action = $(form).action;
			var parameters = $(form).serialize(true);
			if (advancedParameters) parameters+=advancedParameters;
			if(!skipAnim){
			$(element).style.display = 'none';
			}
			new Ajax.Updater(
				$(element),
				action,
				{
					method:"get",
					evalScripts:true,
					parameters:parameters,
					onComplete:function()
                    {
                    if(!skipAnim){
                            Effect.toggle(element,'appear');
                            }
                    }
				}
			)
		},
    ExecuteBackGround : function(event,onComplete){
		var href = $et(event).getAttribute('href');
		href += '&ajax=1';
		new Ajax.Request( href,
			{
				evalScripts:true,
                method:'get',
				onComplete : onComplete
			}		
		);
		return false;
    },
	
	 LoadHidden : function(event,param,refSrc,skipAnim)
       {
            
            var ref = refSrc;
            if(!ref)ref = $et(event);
            var href = ref.getAttribute("href");
             if (param) href+=param;
            
             new Ajax.Request(
                href,
                {
                    method:"get",
                    evalScripts:false
                }
            );
       
       },
	
    LoadContent : function(event,param,refSrc,skipAnim)
       {
            
            var ref = refSrc;
            if(!ref)ref = $et(event);
            var href = ref.getAttribute("href");
             if (param) href+=param;
             if(Data && Data.CurrentEdit)Data.CurrentEdit = null;
            href = href + "&ajax=1";
            if(!skipAnim){
             $('maincontent').style.display = 'none';
             }
             new Ajax.Updater(
                $('maincontent'),
                href,
                {
                    method:"get",
                    evalScripts:true,
                    onComplete:function()
                    {
                            if(!skipAnim){
                            Effect.toggle('maincontent','appear');
                            }
                    }
                }
            );
       
       },
	Update : function(element,command,skipAnim)
       {
            
            command = command + "&ajax=1";
            if(!skipAnim){
             $(element).style.display = 'none';
             }
             new Ajax.Updater(
                $(element),
                command,
                {
                    method:"get",
                    evalScripts:true,
                    onComplete:function()
                    {
                        if(!skipAnim){
                            Effect.toggle(element,'appear');
                            }
                    }
                }
            );
       
       }

}

