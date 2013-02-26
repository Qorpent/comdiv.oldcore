using System;
using System.IO;
using System.Web.Mvc;
using Comdiv;
using Comdiv.Extensions;
using MvcContrib.Comdiv;
using MvcContrib.Comdiv.Layers;
using MvcContrib.Comdiv.ViewEngines.Brail;

namespace MvcContrib.ViewFactories{
    

   

    public class BrailViewFactory : VirtualPathProviderViewEngine, IBrailViewFactory{
        private static BooViewEngine _defaultViewEngine;
        private readonly BooViewEngine _viewEngine;
        public event ResolveViewNameEventHandler OnResolveViewName;
        protected string  resolveViewNameByEvent(string basis, string name){
            if(OnResolveViewName!=null){
                var args = new ResolveViewNameEventArgs(basis, name);
                OnResolveViewName.Invoke(this,args);
                return args.Result;
            }
            return null;
        }

        public event ResolveBrailCodeEventHandler OnResolveCode;
        public string CustomResolveCode(string name){
            if(OnResolveCode!=null){
                var args = new ResolveBrailCodeEventArgs(name);
                OnResolveCode(this, args);
                return args.Result;
            }
            return null;
        }
        public BrailViewFactory()
            : this(DefaultViewEngine) {}

        public string ResolveViewName(string basis, string subview){
            lock (this){
                var result = resolveViewNameByEvent(basis, subview);
                if (null == result){
                    result = VirtualPathProviderLayerExtension.ResolveSubView(this, basis, subview);
                }
                return result;
            }
        }
        public BrailViewFactory(BooViewEngine viewEngine){
            if (viewEngine == null){
                throw new ArgumentNullException("viewEngine");
            }
            this.configureLayers("brail");
            _viewEngine = viewEngine;
            _viewEngine.Factory = this;
            _viewEngine.Initialize();
        }

        public BooViewEngine ViewEngine{
            get { return _viewEngine; }
        }

        private static BooViewEngine DefaultViewEngine{
            get{
                if (_defaultViewEngine == null){
                    _defaultViewEngine = new BooViewEngine();
                }

                return _defaultViewEngine;
            }
        }


        protected override bool FileExists(ControllerContext controllerContext, string virtualPath){
            try{
                return base.FileExists(controllerContext, virtualPath);
            }
            catch{
                return File.Exists(virtualPath.mapPath());
            }
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath){
            return CreateView(controllerContext, partialPath, null);
        }


        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath){
            try{
                return (IView)_viewEngine.Process(
                    viewPath.mapPath().ToLower(),
                    masterPath.hasContent()? masterPath.mapPath().ToLower() : null
                    );
            }
            catch (Exception ex){
                //avoid to throw exception if view not found - in mvc we find them in stack
                if (ex.Message.Contains("is not a valid view")){
                    return null;
                }
                throw;
            }
        }
    }
}