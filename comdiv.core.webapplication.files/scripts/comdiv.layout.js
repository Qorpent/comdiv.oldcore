var zeta = (undefined != zeta) ? zeta : {};
zeta.layout = (undefined != zeta.layout) ? zeta.layout : {};
Object.extend(zeta.layout,{
	init : function() {
		Event.observe(window,'load', function(){
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
			this.content.center.footer = $('zlt_content_center_footer');
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
	clean : function(){
			if(this.header.left.innerHTML.strip() == ""){
				this.header.left.remove();
				this.header.left = null;
			}
			if(this.header.center.innerHTML.strip() == ""){
				this.header.center.remove();
				this.header.center = null;
			}
			if(this.header.right.innerHTML.strip() == ""){
				this.header.right.remove();
				this.header.right = null;
			}
			if(this.header.innerHTML.strip() == ""){
				this.header.remove();
				this.header = null;
			}
			
			if(this.footer.left.innerHTML.strip() == ""){
				this.footer.left.remove();
				this.footer.left = null;
			}
			if(this.footer.center.innerHTML.strip() == ""){
				this.footer.center.remove();
				this.footer.center = null;
			}
			if(this.footer.right.innerHTML.strip() == ""){
				this.footer.right.remove();
				this.footer.right = null;
			}
			if(this.footer.innerHTML.strip() == ""){
				this.footer.remove();
				this.footer = null;
			}
			
			if(this.content.left.innerHTML.strip() == ""){
				this.content.left.remove();
				this.content.left = null;
			}
			if(this.content.right.innerHTML.strip() == ""){
				this.content.right.remove();
				this.content.right = null;
			}
			if(this.content.center.header.innerHTML.strip() == ""){
				this.content.center.header.remove();
				this.content.center.header = null;
			}
			if(this.content.center.footer.innerHTML.strip() == ""){
				this.content.center.footer.remove();
				this.content.center.footer = null;
			}
	}
});

zeta.layout.init();
