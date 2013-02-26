((function(){
	qweb.defineGlobal('comdiv.roles.render');
	Object.extend(comdiv.roles.render,{
		matrix : function(matrix,e){
			matrix = matrix ||  comdiv.roles.getMatrix();
			//cells = [];
			if(!e) e = new Element('div',{id:'__role_matrix',class: 'role_matrix'});
			e = $(e);
			
			t = e.appendChild(new Element('table'));
			s = t.appendChild(new Element('thead'));
			r = s.appendChild(new Element('tr'));
			c = r.appendChild(new Element('th',{rowspan :2 }).update('Пользователи'));
			
			for (i=0;i<matrix.targets.target.length;i++){
				tg = matrix.targets.target[i];
				c = r.appendChild(new Element('th',{colspan : tg.roles.role.length}).update(tg.name+' ('+tg.code+')'));
			}
			r = s.appendChild(new Element('tr'));
			for (i=0;i<matrix.targets.target.length;i++){
				tg = matrix.targets.target[i];
				for (j=0;j<tg.roles.role.length;j++){
					role = tg.roles.role[j];
					c = r.appendChild(new Element('th').update(role.name+'<br/>('+role.code+')'));
				}
			}
			
			s = t.appendChild(new Element('tbody'));
			
			for (i=0;i<matrix.users.user.length;i++){
				u = matrix.users.user[i];
				r = s.appendChild(new Element('tr'));
				c = r.appendChild(new Element('td').update(u.name+' ('+u.code+')'));
				for (j=0;j<matrix.targets.target.length;j++){
					tg = matrix.targets.target[j];
					for (k=0;k<tg.roles.role.length;k++){
						role = tg.roles.role[k];
						c = r.appendChild(new Element('td'));
						c.roleObject = {
							user : u.code,
							target : tg.code,
							role : role.code,
						};
						comdiv.roles.ui.extendCell(c);
						id  =  u.code+'_'+tg.code+'_'+role.code;
						c.setAttribute("id",id);
					}
				}
			}
			
			
			
			comdiv.roles.ui.extendTable(t);
			
			document.body.appendChild(e);
			comdiv.roles.ui.massSetCells();
			return e;
		},
	});
})())