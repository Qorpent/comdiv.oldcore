var zeta = (undefined != zeta) ? zeta : {};
zeta.layout = (undefined != zeta.layout) ? zeta.layout : {};
Object.extend(zeta.layout,{
	init : function() {
		Event.observe(window,'load', function(){
			Ajax.loadCSS("default");
			this.header = $('zlt_header');
			this.header.left = $('zlt_header_left');
			this.header.center = $('zlt_header_center');
			this.header.right = $('zlt_header_right');
			this.content = $('zlt_content');
			this.content.left = $('zlt_content_left');
			this.content.center = $('zlt_content_center');
			this.content.right = $('zlt_content_right');
			this.content.center.header = $('zlt_content_center_header');
			this.content.center.content = $('zlt_content_center_content');
			this.content.center.footer = $('zlt_content_right_footer');
			this.content.center.content.main = $('zlt_content_center_content_main');
			this.content.center.content.extension = $('zlt_content_center_content_extension');
			this.footer = $('zlt_footer');
			this.footer.left = $('zlt_footer_left');
			this.footer.center = $('zlt_footer_center');
			this.footer.right = $('zlt_footer_right');
			this.main = $('zlt_content_center_content_main');
			this.extension = $('zlt_content_center_content_extension');
		}.bind(this));
	},
});

zeta.layout.init();

zeta.fish = (undefined != zeta.fish) ? zeta.fish : {};
Object.extend(zeta.fish,{
	
	init : function() {
		this.header_left();
		this.header_center();
		this.header_right();
		this.content_left();
		this.content_right();
		this.content_center_header();
		this.main();
		this.extension();
		this.footer_center();
	},
	
	header_left : function() {
		var img = new Element('img', {'src': 'img/top_logo.png', 'style': 'padding: 11px 0px 0px 25px;'});
		$('zlt_header_left').appendChild(img);
	},
	
	header_center : function() {
		var ul = new Element('ul', {'class': 'topmenu'});
		var titles = new Array('Стройка', 'Документация', 'Новости', 'Поддержка', 'Пользователи', 'Права', 'Отчеты', 'Курсы', 'Очистка');
		for (var i = 0; i<8; i++) {
			var li = new Element('li');
			var a = new Element('a', {'href': '#'}).update(titles[i]);
			li.appendChild(a);
			ul.appendChild(li);
		}
		$('zlt_header_center').appendChild(ul);
	},
	
	header_right : function() {
		var a1 = new Element('a', {'href': '#'}).update('Ссылка на что то<br/>');
		var a2 = new Element('a', {'href': '#'}).update('Ссылка на что то еще');
		$('zlt_header_right').appendChild(a1);
		$('zlt_header_right').appendChild(a2);
	},
	
	content_left : function() {
		var ul = new Element('ul');
		var titles = new Array('Ссылка 1', 'Ссылка 2', 'Ссылка 3', 'Ссылка 4', 'Ссылка 5', 'Ссылка 6', 'Ссылка 7', 'Ссылка 8', 'Ссылка 9');
		for (var i = 0; i<8; i++) {
			var li = new Element('li');
			var a = new Element('a', {'href': '#'}).update(titles[i]);
			li.appendChild(a);
			ul.appendChild(li);
		}
		$('zlt_content_left').appendChild(ul);
	},
	
	content_right : function() {
		var ul = new Element('ul');
		var titles = new Array('Ссылка 1', 'Ссылка 2', 'Ссылка 3', 'Ссылка 4', 'Ссылка 5', 'Ссылка 6', 'Ссылка 7', 'Ссылка 8', 'Ссылка 9');
		for (var i = 0; i<8; i++) {
			var li = new Element('li');
			var a = new Element('a', {'href': '#'}).update(titles[i]);
			li.appendChild(a);
			ul.appendChild(li);
		}
		$('zlt_content_right').appendChild(ul);
	},
	
	content_center_header : function() {
		
	},
	
	main : function() {
		var p = new Element('p').update('Updated syntax highlighting to distinguish between HTML/XML, CSS and JavaScript<br/>' +
		'Ability to add new rules in the Style Inspector (initial implementation)<br/>' + 
		'Badge displaying the error count in the Error Log button (Opera Dragonfly must be open before<br/>' + 
		'loading the page to see this). A similar badge is show in the JavaScript Debugger button to show when execution is paused<br/>' + 
		'Updated scroll behaviour in the Scripts source view<br/>' + 
		'Visual differentiation to show that the browser default styles in the Style Inspector can not be edited<br/>' + 
		'Edit: Inline and Evaled scripts now show a code snippet in the script drop down<br/>' +
		'The redesign and implementation is still work in progress, but we’d like to share with you where were are currently at.<br/>' +
		'Much of the UI framework design is close to final, while the modes themselves are still in the process of being redesigned.<br/>' +
		'Much of the UX changes are also yet to be implemented, such as the editing discoverability and context menu support');
		$('zlt_content_center_content_main').appendChild(p);
	},
	
	extension : function() {
		var p1 = new Element('p').update('Opera Mini always uses Opera’s advanced server compression technology to compress web content before it gets to a device. The rendering engine is on Opera’s server.');
		var p2 = new Element('p').update('Opera Mobile is a full Internet browser for mobile devices. The full web-rendering engine – Opera Presto – is run on the phone using the phones’ hardware to download and display webpages.');
		$('zlt_content_center_content_extension').appendChild(p1);
		$('zlt_content_center_content_extension').appendChild(p2);
	},
	
	footer_center : function() {
		var p = new Element('p').update('Сделано на платформе COMDIV Zeta 2.9 2007-2011 при поддержке ООО "Медиа технологии" , Apache License 2.0 , rev. 2.9.0.1450');
		$('zlt_footer_center').appendChild(p);
	},
	
	
});


