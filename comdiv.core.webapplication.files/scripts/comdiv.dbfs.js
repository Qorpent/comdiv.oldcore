var dbfs = dbfs || {}
dbfs.upload = dbfs.upload || {}
Object.extend(dbfs,{
	onload : function(){
		$('uploadformprogress').hide();
		Event.observe($('typelist'),'change',function(){
			$('typefield').value = $('typelist').value;
		});
	},
	search : function(p, result){
		
		var params = p || $('searchform').serialize();
		var div = result || 'searchresult';
		$$U(div,'dbfs/search',params);
	},
	remove : function(code,id){
		if(confirm("Вы уверены?")){
			$('_tr'+id).remove();
			$$R('dbfs/delete',{"code":code});
		}
	},	
	edit : function(code, name, tags, alist){
		$('uploadform').reset();
		$('uploadformcodefield').value = code;
		$('uploadformnamefield').value = name;
		$('uploadformaccesslist').value = alist;
		$('uploadformtags').value = tags;
	},
_:0});
Object.extend(dbfs.upload,{
	onstart : function(id){
		$(id+'resultdiv').update('');
		$(id+'progress').show();
		$(id+'submit').disabled=true;
	},
	oncomplete : function(id,html){
		$(id+'resultdiv').update(html);
		$(id+'progress').hide();
		$(id+'submit').disabled=false;
		var tags = $(id+'tags').value;
		var htags = $(id+'hiddentags').value;
		var accs = $(id+'accesslist').value;
		$(id).reset();
		$(id+'tags').value = tags;
		$(id+'accesslist').value = accs;
		$(id+'hiddentags').value = htags;
	},
	
_:0});


Event.observe(window,"load", dbfs.onload);