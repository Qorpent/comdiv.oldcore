function $N(id)
{
	var v = null;
    if($(id).tagName.toUpperCase() == "INPUT")
		v = $F(id);
	else 
		v = $(id).innerHTML.match(/-?\d+\.?\d*/);
    if(v)
    {
        return parseFloat(v.replace(",","."));
    }
    else
        return 0;
}
function $$ev(id)
{
    var result = 0;
    if(id && $(id))
    {
        if(null != $(id).getAttribute('et_formula'))
            result = eval($(id).getAttribute('et_formula'));
        else
            result = $N(id);
    }
    return result;
}

Array.prototype.append = function(o){
    this[this.length] = o;
}


var ETable = 
{
	OverrideKeyDown : true,
    Count : 0,
    TableRoot : null,
    Indexes : {},
    Init : function(id)
    {
       var els = $(id).descendants();
       ETable.TableRoot = $(id);
       ETable.Indexes  = {};
       var cache = [];
       for(var i = 0;i<els.length;i++)
       {
            var e = els[i];
            try{
				if(e.tagName == "INPUT")
				{
					e.observe("change",ETable.ChangeVisualState,false);
					if (e.getAttribute('row'))
					{
						var key = e.getAttribute('row') + '_'+ e.getAttribute('col');
						ETable.Indexes[key] = e;
					}
					if(null != e.getAttribute("et_depend"))
					{
						e.observe("change",ETable.ReEvaluate,false);
	                    
						ETable.EvalDepend(e,cache);
					}
				}
            }catch(e){
				
            }
       } 

    },

    CheckOld : function(event){
	var el = $et(event);
        var old = el.getAttribute("oldValue");
        var val = el.value;
        if (val == "")
        {
            val = old;
            el.value = old;
        }

},
    
    ReEvaluate : function(event)
    {
        var el = $et(event);
       	ETable.CheckOld(event);
        ETable.ChangeVisualState(event)
        
        ETable.EvalDepend(el);
    },
    
    ChangeVisualState : function(event)
    {
         var el = $et(event);
        var old = el.getAttribute("oldValue");
        var val = el.value;
        ETable.CheckOld(event);
        if (val!=old)
        {
            el.classNames().add("changed");
        }
        else
        {
            el.classNames().remove("changed");
        }
    },
    
    EvalDepend : function(el,cache)
    {
        
        var targets = eval(el.getAttribute('et_depend'));
        for(var i = 0; i<targets.length;i++)
        {
            var n = targets[i];
            var e = $(n);
            if(cache && cache.include(e))return;
            if(cache ) cache.push(e);
            ETable.Count++;
            e.update($$ev(n).toFixed(2));
            if(null != e.getAttribute("et_depend"))
            {
                ETable.EvalDepend(e,cache);
            }          
            
            
        }
        
    },
    
    KeyDown : function(event)
    {
        var i = $et(event);
        var result = true;
        var direction = 'down';
        
		if (event.keyCode==13) {
			result = false;
		}else{
			if(ETable.OverrideKeyDown){
				if (event.keyCode==40) result = false;
				else if (event.keyCode==38) {direction='up'; result = false;}
				else if (event.keyCode==37) {direction='left'; result = false;}
				else if (event.keyCode==39) {direction='right'; result = false;}
			}
		}
        if (result) return result;
        ETable.ProcessMove(i,direction);
        event.bubble=false;
        event.cancel=true;
        return false;
    }
    ,
    ProcessMove : function(e,direction)
    {
        var col = e.getAttribute('col');
        var row = e.getAttribute('row');
        switch (direction)
        {
            case "up" : 
                row = parseInt(row) - 1;
                break;
            case "down" : 
                row = parseInt(row) + 1;
                break;
            case "left" : 
                col = parseInt(col) - 1;
                break;
            case "right" : 
                col = parseInt(col) + 1;
                break;
        }
        var t = ETable.Indexes[row+'_'+col];
        if (t)
        {
            t.focus();
            //t.select();
        }
    }
}