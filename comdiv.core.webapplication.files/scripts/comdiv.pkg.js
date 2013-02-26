var comdiv = comdiv || {};
comdiv.pkg = comdiv.pkg || {};
Object.extend(comdiv.pkg, {
	
	head :  $$("head")[0],
	packages : {},
	resourcedependency : {},
	_pkgcache : {},
	_rescache : {},
	scriptroot : _siteroot + "/scripts/",
	styleroot : _siteroot + "/sys/content/style/",
	resourcenamemap : {},
	
	registerresource : function(resource, usingresources){
		this.resourcedependency[resource] = usingresources;
	},
	register : function(pkgname, pkgs, resources){
		this.packages[pkgname] = { p : pkgs || [], r : resources || [] };
	},

	
	load : function(pkgname){
		if(!Object.isUndefined(comdiv.log)){
				comdiv.log.write({source:"comdiv.pkg->load",text:"attempt to load '"+pkgname+"' package"});
			}
		if (Object.isUndefined(this.packages[pkgname])){
			return;
		}
		if(Object.isUndefined(this._pkgcache[pkgname])){
			this.packages[pkgname].p.each(function(pkg){
				if(pkg){
					comdiv.pkg.load(pkg);
				}
			});
			this.packages[pkgname].r.each(function(resource){
				if(resource){
					comdiv.pkg.loadresource(resource);
				}
			});
			
			this._pkgcache[pkgname] = "loaded";
		}
	
	},

	loadresource : function(resource){
		if(Object.isUndefined(this._rescache[resource])){
			if(!Object.isUndefined(this.resourcedependency[resource])){
				this.resourcedependency[resource].each(function(r){
					if(r){
						comdiv.pkg.loadresource(r);
					}
				});
			}
			this.processresource(resource);
			this._rescache[resource] = "loaded";
		}
	
	},

	processresource : function(resource){
		var realname = resource;
		var isjs = /\.js$/.match(resource);
		var iscss = /\.css$/.match(resource);
		if(isjs){
			realname = this.scriptroot + realname;
			if(!Object.isUndefined(this.resourcenamemap[resource])){
				realname = this.resourcenamemap[resource];
			}
			this.processjs(realname);
		}
		if(iscss){
			realname = this.styleroot + realname;
			if(!Object.isUndefined(this.resourcenamemap[resource])){
				realname = this.resourcenamemap[resource];
			}
			this.processcss(realname);
		}
	},

	processjs : function(js){
		var e = new Element("script",{type:"text/javascript",src:js});
		var e2 = new Element("x");
		e2.appendChild(e);
		document.write(e2.innerHTML);
	
	},
	
	processcss : function(css){
		var e = new Element("link",{type:"text/css",rel:"stylesheet",href:css});
		var e2 = new Element("x");
		e2.appendChild(e);
		document.write(e2.innerHTML);
	},
__:null});
