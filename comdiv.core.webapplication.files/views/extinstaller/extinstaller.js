var comdiv = comdiv != undefined ? comdiv : {};
comdiv.extensions = comdiv.extensions != undefined ? comdiv.extensions : {};
Object.extend(comdiv.extensions, {
    install: function (name) {
        Ajax.from("extinstaller", "install").param("name", name).after(function () { document.location = document.location; }).call();
    },
    uninstall: function (name) {
        Ajax.from("extinstaller", "uninstall").param("name", name).after(function () { document.location = document.location; }).call();
    },
    openindex: function (name) {
        window.open(Ajax.siteroot+"/"+name+"/index.rails", "_blank");
    },
});