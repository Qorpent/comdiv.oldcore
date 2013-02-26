// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
// COMDIV:
//  1. NEW: FileName property added to simplify resoving of subviews
//  2. NEW: Factory property added to simplify resolving of subviews
//  3. NEW: caching view render result
//  4. NEW: isempty(object) - intended to use with IEnumerable


using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Comdiv.Extensibility.Brail;
using MvcContrib.ViewFactories;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace MvcContrib.Comdiv.ViewEngines.Brail
#endif
{
    /// <summary>
    ///Base class for all the view scripts, this is the class that is responsible for
    /// support all the behind the scenes magic such as variable to PropertyBag trasnlation, 
    /// resources usage, etc. 
    /// </summary>
    public abstract partial class BrailBase : BrailBaseCommon, IView, IViewDataContainer{
        protected IController __controller;


//		protected IEngineContext context;

        /// <summary>
        /// used to hold the ComponentParams from the view, so their views/sections could access them
        /// </summary>
//		private IList viewComponentsParameters;
        protected IBooViewEngine engine;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="BrailBase"/> class.
        /// </summary>
        /// <param name="viewEngine">The view engine.</param>
        /// <param name="output">The output.</param>
        public BrailBase(IBooViewEngine viewEngine)
//		(BooViewEngine viewEngine, TextWriter output, IEngineContext context, IController __controller, IControllerContext __controllerContext)
        {
            this.engine = viewEngine;
        }

        public BrailViewFactory Factory { get; set; }

        protected ViewContext __viewContext;

        /// <summary>
        /// Initialize all the properties that a script may need
        /// One thing to note here is that resources are wrapped in ResourceToDuck wrapper
        /// to enable easy use by the script
        /// </summary>
        /// 
        public HtmlHelper Html
        {
            get { return (HtmlHelper)Properties["html"]; }
        }

        public UrlHelper Url
        {
            get { return (UrlHelper)Properties["url"]; }
        }

        protected override IBooViewEngine engineByInterface
        {
            get { return engine; }
        }

        ///// <summary>
        /////The path of the script, this is filled by AddBrailBaseClassStep
        ///// and is used for sub views 
        ///// </summary>
        //public virtual string ScriptDirectory
        //{
        //    get { return viewEngine.ViewRootDir; }
        //}

        public ViewContext ViewContext{
            get { return __viewContext; }
            set { __viewContext = value; }
        }

        /// <summary>
        /// Gets the view engine.
        /// </summary>
        /// <value>The view engine.</value>
        public IBooViewEngine ViewEngine{
            get { return engine; }
        }

        /// <summary>
        /// Gets the flash.
        /// </summary>
        /// <value>The flash.</value>
        public TempDataDictionary Flash{
            get { return __viewContext.TempData; }
        }

        #region IView Members



        public void Render(ViewContext viewContext, TextWriter writer){
            __controller = viewContext.Controller;
            __viewContext = viewContext;
            initProperties(__viewContext);
            Render(writer);
        }

        #endregion

        

        #region IViewDataContainer Members

        public ViewDataDictionary ViewData{
            get { return ViewContext.ViewData; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        protected override void renderSubView(BrailBaseCommon subView, TextWriter writer) {
            ((IView)subView).Render(__viewContext, writer);
        }

        

        protected virtual void initProperties(ViewContext viewContext){
            //properties.Add("dsl", new DslWrapper(this));
            properties.Add("Controller", viewContext.Controller);

            HttpContextBase myContext = viewContext.HttpContext;

            //in standalone or test call we uses EmptyHttpContext which is not public, but here we must avoid it
            if (myContext.GetType().Name == "EmptyHttpContext"){
                myContext = null;
            }

            properties.Add("Context", myContext);
            if (myContext != null){
                properties.Add("request", myContext.Request);
                properties.Add("response", myContext.Response);
                properties.Add("session", myContext.Session);
            }

            properties["html"] = new HtmlHelper(viewContext, this);
            properties["url"] = new UrlHelper(viewContext.RequestContext);

            if (myContext != null && myContext.Request.QueryString != null){
                foreach (string key in myContext.Request.QueryString.AllKeys){
                    if (key == null){
                        continue;
                    }
                    properties[key] = myContext.Request.QueryString[key];
                }
            }

            if (myContext != null && myContext.Request.Form != null){
                foreach (string key in myContext.Request.Form.AllKeys){
                    if (key == null){
                        continue;
                    }
                    properties[key] = myContext.Request.Form[key];
                }
            }

            if (viewContext.TempData != null){
                foreach (var entry in viewContext.TempData){
                    properties[entry.Key] = entry.Value;
                }
            }
            if (viewContext.ViewData != null){
                foreach (var entry in viewContext.ViewData){
                    if (!properties.ContainsKey(entry.Key)){
                        properties[entry.Key] = entry.Value;
                    }
                }
                properties["viewData"] = viewContext.ViewData;
            }


            if (myContext != null){
                string siteRoot = myContext.Request.ApplicationPath;
                if (siteRoot != null && !siteRoot.EndsWith("/")){
                    siteRoot += "/";
                }

                properties["siteRoot"] = siteRoot;
            }
        }

    }
}