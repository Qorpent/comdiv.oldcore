var comdiv = comdiv || {};
comdiv.log = comdiv.pkg || {};
Object.extend(comdiv.log, {
	items : [],
	write: function(logitem){
		comdiv.log.items.push(logitem);
	},
	clear : function(logitem){
		comdiv.log.items = [];
	},
	show : function(e){
		if(!e){
			e = new Element("div",{id:"logdiv","class":"logdiv"});
			e.appendChild(new Element("input",{type:"button",onclick:"$('logdiv').remove()",value:"Закрыть"}));
			e2 = new Element("div",{"class":"loginner"});
			e.appendChild(e2);
			document.body.appendChild(e);
			e.show();
			e = e2;
			
		}
		e = $(e);
		comdiv.log.items.each(function(i){
			var div = new Element("div",{"class":"logitem"});
			if(Object.isUndefined(i.onrender)){
				div.update(i.text);
			}else{
				i.onrender(div);
			}
			e.appendChild(div);
		});
	},
_:0});
